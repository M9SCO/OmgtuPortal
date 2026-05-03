import Keycloak from 'keycloak-js'

const keycloak = new Keycloak({
  url: import.meta.env.VITE_KEYCLOAK_URL || 'http://localhost:8080',
  realm: import.meta.env.VITE_KEYCLOAK_REALM || 'upomgtu',
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'upomgtu',
})

export async function initAuth(): Promise<boolean> {
  const authenticated = await keycloak.init({
    onLoad: 'login-required',
    checkLoginIframe: false,
  })

  if (authenticated) {
    // Auto-refresh token before it expires
    setInterval(async () => {
      try {
        await keycloak.updateToken(30)
      } catch {
        keycloak.login()
      }
    }, 10_000)
  }

  return authenticated
}

export function getToken(): string | undefined {
  return keycloak.token
}

export function logout(): void {
  keycloak.logout({ redirectUri: window.location.origin })
}

export { keycloak }
