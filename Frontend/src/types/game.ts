export enum GameType {
  WordHidden = 0,
  Questions = 1,
}

export enum GameState {
  Lobby = 0,
  Game = 1,
  Ended = 2,
}

export enum RoundState {
  Waiting = 0,
  Active = 1,
  Voting = 2,
  Ended = 3,
}

export interface CreateGameRequest {
  gameType: GameType;
  maxPlayers: number;
  impostorCount: number;
  timerDuration: number;
  maxRounds: number;
}

export interface JoinGameRequest {
  lobbyCode: string;
}

export interface SubmitAnswerRequest {
  answer: string;
}

export interface SubmitVoteRequest {
  targetPlayerId: string;
  reason?: string;
}

export interface PlayerResponse {
  id: string;
  userId: string;
  isImpostor: boolean;
  isReady: boolean;
  isEliminated: boolean;
  score: number;
  joinedAt: string;
  showImpostorStatus: boolean;
}

export interface AnswerResponse {
  id: string;
  playerId: string;
  value: string;
  createdAt: string;
  isEdited: boolean;
}

export interface VoteResponse {
  id: string;
  voterId: string;
  targetId: string;
  createdAt: string;
  reason?: string;
}

export interface RoundResponse {
  id: string;
  roundNumber: number;
  state: RoundState;
  startedAt: string;
  endedAt?: string;
  timeLimit: number;
  remainingTime: number;
  questionText?: string;
  word?: string;
  answers: AnswerResponse[];
  votes: VoteResponse[];
  hasPlayerAnswered: boolean;
  hasPlayerVoted: boolean;
}

export interface GameStateResponse {
  id: string;
  lobbyCode: string;
  hostId: string;
  maxPlayers: number;
  currentPlayers: number;
  type: GameType;
  state: GameState;
  roundNumber: number;
  maxRounds: number;
  timerDuration: number;
  createdAt: string;
  startedAt?: string;
  players: PlayerResponse[];
  currentRound?: RoundResponse;
}

export interface ApiResponse<T> {
  message: string;
  game?: T;
}
