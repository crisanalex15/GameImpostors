import axios from "axios";
import {
  CreateGameRequest,
  JoinGameRequest,
  SubmitAnswerRequest,
  SubmitVoteRequest,
  GameStateResponse,
  ApiResponse,
} from "../types/game";

// const API_BASE_URL = "http://18.196.173.184:5086/api";
const API_BASE_URL = "http://localhost:5001/api";

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add auth token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("authToken");
  console.log(
    "API Request:",
    config.url,
    "Token:",
    token ? "Present" : "Missing"
  );
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
    console.log("Authorization header set:", config.headers.Authorization);
  }
  return config;
});

// Add response interceptor for debugging
api.interceptors.response.use(
  (response) => {
    console.log(
      "API Response:",
      response.config.url,
      "Status:",
      response.status
    );
    return response;
  },
  (error) => {
    console.log(
      "API Error:",
      error.config?.url,
      "Status:",
      error.response?.status,
      "Message:",
      error.response?.data
    );
    return Promise.reject(error);
  }
);

// Auth API functions
export const authApi = {
  // Get user profile
  getProfile: async (): Promise<ApiResponse<any>> => {
    const response = await api.get("/Auth/profile");
    return response.data;
  },

  // Update username
  updateUsername: async (request: {
    username: string;
  }): Promise<ApiResponse<any>> => {
    const response = await api.post("/Auth/Update-username", request);
    return response.data;
  },

  // Change password
  changePassword: async (request: {
    currentPassword: string;
    newPassword: string;
  }): Promise<ApiResponse<any>> => {
    const response = await api.post("/Auth/change-password", request);
    return response.data;
  },

  // Logout
  logout: async (): Promise<ApiResponse<any>> => {
    const response = await api.post("/Auth/logout");
    return response.data;
  },

  // Login
  login: async (request: {
    email: string;
    password: string;
  }): Promise<ApiResponse<any>> => {
    const response = await api.post("/Auth/login", request);
    return response.data;
  },

  // Register
  register: async (request: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }): Promise<ApiResponse<any>> => {
    const response = await api.post("/Auth/register", request);
    return response.data;
  },

  // Verify email with code
  verifyEmail: async (request: {
    email: string;
    code: string;
  }): Promise<ApiResponse<any>> => {
    const response = await api.post("/Auth/verify-email-with-code", request);
    return response.data;
  },

  // Forgot password
  forgotPassword: async (request: {
    email: string;
  }): Promise<ApiResponse<any>> => {
    const response = await api.post("/Auth/forgot-password", request);
    return response.data;
  },

  // Reset password
  resetPassword: async (request: {
    email: string;
    code: string;
    newPassword: string;
  }): Promise<ApiResponse<any>> => {
    const response = await api.post("/Auth/reset-password", request);
    return response.data;
  },

  // Refresh token
  refreshToken: async (request: {
    token: string;
    refreshToken: string;
  }): Promise<ApiResponse<any>> => {
    const response = await api.post("/Auth/refresh-token", request);
    return response.data;
  },
};

export const gameApi = {
  // Lobby Management
  // Create a new game
  createGame: async (
    request: CreateGameRequest
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post("/Game/create", request);
    return response.data;
  },

  // Join a game
  joinGame: async (
    request: JoinGameRequest
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post("/Game/join", request);
    return response.data;
  },

  // Leave a game
  leaveGame: async (gameId: string): Promise<{ message: string }> => {
    const response = await api.post(`/Game/${gameId}/leave`);
    return response.data;
  },

  // Set a player ready
  setReady: async (
    gameId: string,
    isReady: boolean
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post(`/Game/${gameId}/ready`, isReady);
    return response.data;
  },

  // Start a game
  startGame: async (
    gameId: string
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post(`/Game/${gameId}/start`);
    return response.data;
  },

  // Get the state of a game
  getGameState: async (gameId: string): Promise<GameStateResponse> => {
    const response = await api.get(`/Game/${gameId}`);
    return response.data;
  },

  // Round Management
  // Submit an answer
  submitAnswer: async (
    roundId: string,
    request: SubmitAnswerRequest
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post(`/Game/round/${roundId}/answer`, request);
    return response.data;
  },

  // Submit a vote
  submitVote: async (
    roundId: string,
    request: SubmitVoteRequest
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post(`/Game/round/${roundId}/vote`, request);
    return response.data;
  },
};

export default api;
