import Keycloak from 'keycloak-js'

const ALL_ROLES = ['admin', 'user', 'student', 'teacher']

let autoLogin = false
let keycloak: Keycloak | null = null

export async function initAuth(): Promise<boolean> {
  const res = await fetch('/api/auth/config')
  const config = await res.json()
  autoLogin = config.autoLogin

  if (autoLogin) return true

  keycloak = new Keycloak({
    url: import.meta.env.VITE_KEYCLOAK_URL || 'http://localhost:8080',
    realm: import.meta.env.VITE_KEYCLOAK_REALM || 'upomgtu',
    clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'upomgtu',
  })

  let authenticated = false
  try {
    authenticated = await keycloak.init({
      onLoad: 'login-required',
      checkLoginIframe: false,
      responseMode: 'query',
    })
  } finally {
    const url = new URL(window.location.href)
    const keycloakParams = ['state', 'session_state', 'code', 'iss']
    keycloakParams.forEach((p) => url.searchParams.delete(p))
    url.hash = ''
    window.history.replaceState(null, '', url.pathname + url.search)
  }

  if (authenticated) {
    setInterval(async () => {
      try {
        await keycloak!.updateToken(30)
      } catch {
        keycloak!.login()
      }
    }, 10_000)
  }

  return authenticated
}

export function getToken(): string | undefined {
  if (autoLogin) return undefined
  return keycloak?.token
}

export function logout(): void {
  if (autoLogin) {
    window.location.reload()
    return
  }
  keycloak?.logout({ redirectUri: window.location.origin })
}

export function getUserRoles(): string[] {
  if (autoLogin) return ALL_ROLES
  return keycloak?.realmAccess?.roles ?? []
}
