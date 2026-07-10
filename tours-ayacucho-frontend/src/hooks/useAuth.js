import { useAuth } from '../context/AuthContext'

/**
 * Hook to access authentication state and actions.
 * Re-exports useAuth from AuthContext for convenience.
 */
export const useAuthHook = () => {
  return useAuth()
}

export default useAuthHook
