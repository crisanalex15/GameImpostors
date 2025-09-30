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
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

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
