import json
import sys
from pathlib import Path


def sum_usage(jsonl_path: Path) -> dict:
    """Sum token usage from a single JSONL file."""
    totals = {"input_tokens": 0, "cache_creation_input_tokens": 0,
              "cache_read_input_tokens": 0, "output_tokens": 0}
    for line in jsonl_path.read_text().strip().splitlines():
        try:
            rec = json.loads(line)
        except json.JSONDecodeError:
            continue
        msg = rec.get("message", {})
        if isinstance(msg, dict) and "usage" in msg:
            u = msg["usage"]
            for key in totals:
                totals[key] += u.get(key, 0)
    return totals


def collect_usage(jsonl_path: Path) -> dict:
    """Sum token usage from main session + all subagent logs."""
    totals = sum_usage(jsonl_path)

    session_id = jsonl_path.stem
    subagents_dir = jsonl_path.parent / session_id / "subagents"
    if subagents_dir.is_dir():
        for sub_file in subagents_dir.glob("*.jsonl"):
            sub_totals = sum_usage(sub_file)
            for key in totals:
                totals[key] += sub_totals[key]

    totals["total_input"] = (totals["input_tokens"]
                             + totals["cache_creation_input_tokens"]
                             + totals["cache_read_input_tokens"])
    totals["total_output"] = totals["output_tokens"]
    return totals


def calc_weighted_cost(usage: dict) -> float:
    """Weighted token cost (dimensionless). Coefficients = API pricing
    per MTok (3 / 3.75 / 0.3 / 15) used as-is without currency label."""
    return (
        usage["input_tokens"] / 1_000_000
        + usage["cache_creation_input_tokens"] * 1.25 / 1_000_000
        + usage["cache_read_input_tokens"] * 0.1 / 1_000_000
        + usage["output_tokens"] * 5.0 / 1_000_000
    )


def format_usage(usage: dict) -> str:
    weighted = calc_weighted_cost(usage)
    return (
        f"input: {usage['input_tokens']:,} "
        f"| cache_create: {usage['cache_creation_input_tokens']:,} "
        f"| cache_read: {usage['cache_read_input_tokens']:,} "
        f"| output: {usage['output_tokens']:,} "
        f"| **стоимость: {weighted:.2f} у.е.**"
    )


def parse_jsonl(path: str) -> str:
    jsonl_path = Path(path)
    lines = jsonl_path.read_text().strip().splitlines()
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

    # Append token usage summary
    usage = collect_usage(jsonl_path)
    output.append("\n---\n")
    output.append(f"**Токены:** {format_usage(usage)}")

    return "\n".join(output)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("usage: python parser.py <file.jsonl> [output.md]")
        sys.exit(1)

    result = parse_jsonl(sys.argv[1])

    if len(sys.argv) >= 3:
        Path(sys.argv[2]).write_text(result)
        print(f"saved to {sys.argv[2]}")
    else:
        print(result)
