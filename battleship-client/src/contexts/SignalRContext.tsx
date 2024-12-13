import React, { createContext, useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { Player, Game, GameState } from '../models';

interface SignalRContextType {
    connection: signalR.HubConnection | null;
    players: Player[];
    isGameStarted: boolean;
    game: Game | null;
    gameState: GameState | null;
    isLoading: boolean; // Add this property
    setPlayerReady: (gameId: string, playerId: string) => Promise<void>;
    updatePlayers: (newPlayers: Player[]) => void;
}


export const SignalRContext = createContext<SignalRContextType | undefined>(undefined);

export const SignalRProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [players, setPlayers] = useState<Player[]>([]);
    const [isGameStarted, setIsGameStarted] = useState<boolean>(false);
    const [gameState, setGameState] = useState<GameState | null>(null);
    const [game, setGame] = useState<Game | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true); // Add this state

    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("https://battle-ship-api-bjb4g6bqhuaubcad.northeurope-01.azurewebsites.net/gameHub", {
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling | signalR.HttpTransportType.ServerSentEvents,
                withCredentials: true,
            })
            .withAutomaticReconnect()
            .build();
    
        setConnection(newConnection);
    
        const startConnection = async (attempt = 1) => {
            try {
                console.log(`Attempting to start SignalR connection (Attempt ${attempt})...`);
                await newConnection.start();
                console.log("Connected to SignalR successfully.");
                setIsLoading(false); // Connection established
            } catch (err) {
                console.error("SignalR connection failed:", err);
                const delay = Math.min(1000 * 2 ** attempt, 30000);
                console.log(`Retrying SignalR connection in ${delay / 1000} seconds...`);
                setTimeout(() => startConnection(attempt + 1), delay);
            }
        };
    
        startConnection();
    
        newConnection.on("PlayerReady", (playerId: string) => {
            setPlayers((prevPlayers) =>
                prevPlayers.map((player) =>
                    player.id === playerId ? { ...player, isReady: true } : player
                )
            );
        });
    
        newConnection.on("GameStarted", (state: GameState, game: Game) => {
            console.log("Game has started!");
            setIsGameStarted(true);
            setGameState(state);
            setGame(game);
        });
    
        newConnection.on("UpdateGameState", (game: Game) => {
            console.log("Game state updated:", game);
            setGame(game);
        });
    
        return () => {
            newConnection.stop();
        };
    }, []);

    const setPlayerReady = async (gameId: string, playerId: string) => {
        if (connection) {
            try {
                await connection.invoke("SetPlayerReady", gameId, playerId);
            } catch (err) {
                console.error("Failed to set player ready:", err);
            }
        }
    };

    const updatePlayers = (newPlayers: Player[]) => {
        setPlayers(newPlayers);
    };

    return (
        <SignalRContext.Provider 
            value={{ 
                connection, 
                players, 
                isGameStarted, 
                gameState, 
                isLoading, // Ensure this is included
                setPlayerReady, 
                updatePlayers, 
                game 
            }}
        >
            {children}
        </SignalRContext.Provider>
    );
};
