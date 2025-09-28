import axios from "axios";
import {
  CreateGameRequest,
  JoinGameRequest,
  SubmitAnswerRequest,
  SubmitVoteRequest,
  GameStateResponse,
  ApiResponse,
} from "../types/game";

const API_BASE_URL = "http://localhost:5086/api";

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add auth token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("authToken");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const gameApi = {
  // Lobby Management
  createGame: async (
    request: CreateGameRequest
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post("/game/create", request);
    return response.data;
  },

  joinGame: async (
    request: JoinGameRequest
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post("/game/join", request);
    return response.data;
  },

  leaveGame: async (gameId: string): Promise<{ message: string }> => {
    const response = await api.post(`/game/${gameId}/leave`);
    return response.data;
  },

  setReady: async (
    gameId: string,
    isReady: boolean
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post(`/game/${gameId}/ready`, isReady);
    return response.data;
  },

  startGame: async (
    gameId: string
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post(`/game/${gameId}/start`);
    return response.data;
  },

  getGameState: async (gameId: string): Promise<GameStateResponse> => {
    const response = await api.get(`/game/${gameId}`);
    return response.data;
  },

  // Round Management
  submitAnswer: async (
    roundId: string,
    request: SubmitAnswerRequest
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post(`/game/round/${roundId}/answer`, request);
    return response.data;
  },

  submitVote: async (
    roundId: string,
    request: SubmitVoteRequest
  ): Promise<ApiResponse<GameStateResponse>> => {
    const response = await api.post(`/game/round/${roundId}/vote`, request);
    return response.data;
  },
};

export default api;
