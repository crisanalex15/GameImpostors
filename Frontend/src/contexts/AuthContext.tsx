import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
} from "react";

interface User {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
}

interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (token: string, user: User) => void;
  logout: () => void;
  isAuthenticated: boolean;
  currentGameId: string | null;
  setCurrentGameId: (gameId: string | null) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [currentGameId, setCurrentGameId] = useState<string | null>(null);

  useEffect(() => {
    const storedToken = localStorage.getItem("authToken");
    const storedUser = localStorage.getItem("user");
    const storedGameId = localStorage.getItem("currentGameId");

    if (storedToken && storedUser) {
      setToken(storedToken);
      setUser(JSON.parse(storedUser));
    }

    if (storedGameId) {
      setCurrentGameId(storedGameId);
    }
  }, []);

  const login = (newToken: string, newUser: User) => {
    setToken(newToken);
    setUser(newUser);
    localStorage.setItem("authToken", newToken);
    localStorage.setItem("user", JSON.stringify(newUser));

    // Verifică dacă există un gameId salvat și îl setează
    const storedGameId = localStorage.getItem("currentGameId");
    if (storedGameId) {
      setCurrentGameId(storedGameId);
    }
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    setCurrentGameId(null);
    localStorage.removeItem("authToken");
    localStorage.removeItem("user");
    localStorage.removeItem("currentGameId");
  };

  const handleSetCurrentGameId = (gameId: string | null) => {
    setCurrentGameId(gameId);
    if (gameId) {
      localStorage.setItem("currentGameId", gameId);
    } else {
      localStorage.removeItem("currentGameId");
    }
  };

  const isAuthenticated = !!token && !!user;

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        login,
        logout,
        isAuthenticated,
        currentGameId,
        setCurrentGameId: handleSetCurrentGameId,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
