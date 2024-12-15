import React, { useState, useContext, useEffect } from 'react';
import { Player, EGame } from '../models';
import Board from './Board';
import { SignalRContext } from '../contexts/SignalRContext';
import '../style/GameBoard.css';

interface GameBoardProps {
    players: Player[];
    currentPlayerId: string | null;
    onShipsPlaced: () => void;
}

const GameBoard: React.FC<GameBoardProps> = ({ players, currentPlayerId, onShipsPlaced }) => {
    const [gameStarted, setGameStarted] = useState(false);
    const [isCurrentTurn, setIsCurrentTurn] = useState(false);
    const [game, setGame] = useState<EGame | null>(null);
    const [scores, setScores] = useState<{ [key: string]: number }>({});
    const [selectedAttack, setSelectedAttack] = useState<string>("regular"); // Track selected attack type
    const [isPauseButtonVisible, setPauseButtonVisible] = useState(true);
    const [isResumeButtonVisible, setResumeButtonVisible] = useState(false);
    const { connection } = useContext(SignalRContext)!;
    const currentPlayer = players.find(player => player.id === currentPlayerId);
    const currentTeam = currentPlayer?.team;
    const [gameOver, setGameOver] = useState(false);
    const [gameOverMessage, setGameOverMessage] = useState("");

    const handlePause = async () => {
        try {
            await connection?.invoke("PauseGame", "game-1");
            setPauseButtonVisible(false);
            setResumeButtonVisible(true);
        } catch (error) {
            console.error("Error pausing the game:", error);
        }
    };
    
    const handleResume = async () => {
        try {
            await connection?.invoke("ResumeGame", "game-1");
            setResumeButtonVisible(false);
            setPauseButtonVisible(true);
        } catch (error) {
            console.error("Error resuming the game:", error);
        }
    };

    useEffect(() => {
        connection?.on("GameOver", (message: string) => {
            setGameOver(true); 
            setGameOverMessage(message); 
        });
    
        return () => {
            connection?.off("GameOver");
        };
    }, [connection]);

    useEffect(() => {
        connection?.on("GamePaused", (game: EGame) => {
            setGame(game);
            setIsCurrentTurn(false);
            setPauseButtonVisible(false);
            setResumeButtonVisible(true);
            console.log("Game is paused.");
        });
    
        connection?.on("GameResumed", (game: EGame) => {
            setGame(game);
            setIsCurrentTurn(game.currentTurn === currentTeam);
            setPauseButtonVisible(true);
            setResumeButtonVisible(false);
            console.log("Game has resumed.");
        });
    
        return () => {
            connection?.off("GamePaused");
            connection?.off("GameResumed");
        };
    }, [connection, currentTeam]);
    useEffect(() => {
        connection?.on("GameStarted", (gameState: EGame) => {
            setGameStarted(true);
            setGame(gameState);
            setIsCurrentTurn(gameState.currentTurn === currentTeam);
        });

        connection?.on("UpdateGameState", (updatedGame: EGame) => {
            setGame(updatedGame);
            setIsCurrentTurn(updatedGame.currentTurn === currentTeam);
        });

        connection?.on("MoveResult", (moveResult) => {
            const { PlayerId, Row, Col, Result } = moveResult;
        
            // Update the correct board based on PlayerId
            updateBoardCell(PlayerId, Row, Col, Result);
        });
        
        connection?.on("ReceiveGameMode", function (gameMode) {
            // Here, you can update the frontend UI with the received game mode.
            console.log("Game Mode received: ", gameMode);
        });
        

        connection?.on("ReceiveUpdatedScore", (playerId: string, score: number) => {
            setScores(prevScores => ({
                ...prevScores,
                [playerId]: score
            }));
        });

        return () => {
            connection?.off("GameStarted");
            connection?.off("MoveResult");
            connection?.off("ReceiveUpdatedScore");
        };
    }, [connection, currentTeam]);

    function updateBoardCell(playerId: string, row: number, col: number, result: "Hit" | "Miss" | "Sunk") {
        const cell = document.getElementById(`cell-${playerId}-${row}-${col}`); // Lookup cell by playerId
        if (!cell) return;
      
        if (result === "Hit") {
          cell.classList.add("isHit");
        } else if (result === "Sunk") {
          cell.classList.add("sunk");
        } else if (result === "Miss") {
          cell.classList.add("isMissed");
        }
      }
    

    // Updated handleShoot function to include attack type
    const handleShoot = async (row: number, col: number, targetPlayerId: string) => {
        if (!isCurrentTurn || !currentPlayerId || targetPlayerId === currentPlayerId) return;
    
        try {
            const turnResult = await connection?.invoke("MakeMove", "game-1", currentPlayerId, targetPlayerId, row, col, selectedAttack);
    
            if (turnResult) {
                const { TargetPlayerId, AffectedCoordinates, Results } = turnResult;
    
                // Update the game state using the TurnResult
                setGame((prevGame) => {
                    if (!prevGame) return prevGame;
    
                    const updatedGame = { ...prevGame };
                    const targetPlayer = updatedGame.players[TargetPlayerId];
    
                    if (targetPlayer) {
                        AffectedCoordinates.forEach((coord: { Row: number; Col: number }, index: number) => {
                            const result = Results[index];
                            targetPlayer.board.grid[coord.Row][coord.Col].isHit = result === "Hit";
                            targetPlayer.board.grid[coord.Row][coord.Col].isMiss = result === "Miss";
                        });
                    }
    
                    return updatedGame;
                });
            }
        } catch (error) {
            console.error("Error shooting at target:", error);
        }
    };
    
    

    // Function to get a player's board by their ID
    const getPlayerBoard = (playerId: string) => {
        return game?.players?.[playerId]?.board || null;
    };

    const getTeammate = (currentPlayerId: string) => {
        if (currentPlayer && game && game.players) {
            return Object.values(game.players).find(
                player => player.team === currentPlayer.team && player.id !== currentPlayerId
            );
        }
        return null; 
    };

    const getTeamColor = (teamName: string) => {
        return teamName === "Red" ? "#de6f81" : "#6377f7";
    };

    const teammate = currentPlayerId ? getTeammate(currentPlayerId) : null;

    return (
        <div className="game-board-container">
            {gameOver ? (
                // Game Over Screen
                <div className="game-over-screen">
                    <h1>Game Over</h1>
                    <p>{gameOverMessage}</p>
                </div>
            ) : (
                <>
                    <h2>Game Board for {game?.mode} mode</h2>
                    {game?.state === "InProgress" && isPauseButtonVisible && (
                        <button id="pauseButton" onClick={handlePause}>Pause Game</button>
                    )}
                    {/* Check if the game is paused */} 
                    {game?.state === "Paused" ? (
                        <>
                            <p>The game has been paused.</p>
                            {isResumeButtonVisible && <button id="resumeButton" onClick={handleResume}>Resume Game</button>}
                        </>
                    ) : (
                        <>
                            {gameStarted && <p>The game has started!</p>}
                            {isCurrentTurn ? (
                                <>
                                    <p>It's your turn!</p>
                                    {game?.mode !== 'Tournament' && (
                                        <div className="attack-buttons">
                                            <button onClick={() => setSelectedAttack("regular")}>Regular Attack</button>
                                            <button onClick={() => setSelectedAttack("smallbomb")}>Small Bomb</button>
                                            <button onClick={() => setSelectedAttack("bigbomb")}>Big Bomb</button>
                                            <button onClick={() => setSelectedAttack("megabomb")}>Super Bomb</button>
                                            <p>Selected Attack: {selectedAttack}</p>
                                        </div>
                                    )}
                                </>
                            ) : (
                                <p>Waiting for {game?.currentTurn} to play...</p>
                            )}
    
                            {/* Boards */}
                            <div className="boards-container">
                                <div className="board-row">
                                    {currentPlayer && (
                                        <>
                                            <div key={currentPlayer.id} className="board-wrapper" style={{backgroundColor: getTeamColor(currentPlayer.team)}}>
                                                <div className="board-title">{currentPlayer.name}'s Board</div>
                                                {currentPlayer.board && (
                                                    <Board
                                                        board={currentPlayer.board.grid}
                                                        isPlayerBoard={true}
                                                        isTeammateBoard={false}
                                                        onShipsPlaced={onShipsPlaced}
                                                        onShoot={undefined}
                                                        playerName={currentPlayer.name}
                                                        playerId={currentPlayer.id}
                                                        gameId="game-1"
                                                        score={scores[currentPlayer.id] || 0}
                                                        team={getTeamColor(currentPlayer.team)}
                                                    />
                                                )}
                                            </div>
    
                                            {teammate && (
                                                <div key={teammate.id} className="board-wrapper" style={{backgroundColor: getTeamColor(teammate.team)}}>
                                                    <div className="board-title">{teammate.name}'s Board</div>
                                                    {teammate.board && (
                                                        <Board
                                                            board={teammate.board.grid}
                                                            isPlayerBoard={false}
                                                            isTeammateBoard={true}
                                                            onShipsPlaced={undefined}
                                                            onShoot={undefined}
                                                            playerName={teammate.name}
                                                            playerId={teammate.id}
                                                            gameId="game-1"
                                                            score={scores[teammate.id] || 0}
                                                            team={getTeamColor(teammate.team)}
                                                        />
                                                    )}
                                                </div>
                                            )}
                                        </>
                                    )}
                                </div>
    
                                <div className="board-row">
                                    {players.map((player) => {
                                        const isOpponentBoard = player.team !== currentTeam;
                                        const board = getPlayerBoard(player.id) || player.board;
                                        const score = scores[player.id] || 0;
    
                                        return (
                                            isOpponentBoard && (
                                                <div key={player.id} className="board-wrapper" style={{backgroundColor: getTeamColor(player.team)}}>
                                                    <div className="board-title">{player.name}'s Board</div>
                                                    {board && (
                                                        <Board
                                                            board={board.grid}
                                                            isPlayerBoard={false}
                                                            isTeammateBoard={false}
                                                            onShipsPlaced={undefined}
                                                            onShoot={isCurrentTurn ? (row, col) => handleShoot(row, col, player.id) : undefined}
                                                            playerName={player.name}
                                                            playerId={player.id}
                                                            gameId="game-1"
                                                            score={score}
                                                            team={getTeamColor(player.team)}
                                                        />
                                                    )}
                                                </div>
                                            )
                                        );
                                    })}
                                </div>
                            </div>
                        </>
                    )}
                </>
            )}
        </div>
    );    
};

export default GameBoard;
