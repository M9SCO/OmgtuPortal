import json
import sys
from pathlib import Path


def parse_jsonl(path: str) -> str:
    lines = Path(path).read_text().strip().splitlines()
    records = []
    for line in lines:
        try:
            records.append(json.loads(line))
        except json.JSONDecodeError:
            continue

    output = []
    iteration = 0

    for rec in records:
        msg = rec.get("message", {})
        role = msg.get("role")
        content = msg.get("content", "")

        if role == "user":
            if isinstance(content, str):
                iteration += 1
                if iteration == 1:
                    output.append(f"# Задача\n\n{content.strip()}\n")
                else:
                    output.append(f"\n---\n\n**[Итерация {iteration - 1}] Оператор:**\n\n{content.strip()}\n")
            elif isinstance(content, list):
                for block in content:
                    if block.get("type") == "tool_result":
                        result = block.get("content", "")
                        if isinstance(result, str) and result.strip():
                            output.append(f"\n> `tool_result`: {result.strip()[:300]}\n")

        elif role == "assistant":
            if isinstance(content, list):
                for block in content:
                    if block.get("type") == "text":
                        text = block.get("text", "").strip()
                        if text:
                            output.append(f"\n**Агент:**\n\n{text}\n")
                    elif block.get("type") == "tool_use":
                        name = block.get("name", "")
                        inp = block.get("input", {})
                        cmd = inp.get("command") or inp.get("description") or str(inp)[:200]
                        output.append(f"\n> `{name}`: {cmd}\n")
                    elif block.get("type") == "thinking":
                        pass

    return "\n".join(output)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("usage: python parse_experiment.py <file.jsonl> [output.md]")
        sys.exit(1)

    result = parse_jsonl(sys.argv[1])

    if len(sys.argv) >= 3:
        Path(sys.argv[2]).write_text(result)
        print(f"saved to {sys.argv[2]}")
    else:
        print(result)
