import React, { useState, useContext, useEffect } from 'react';
import { Player, GameState, Ship, Game, EGame } from '../models';
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
    const [gameState, setGameState] = useState<GameState | null>(null);
    const [game, setGame] = useState<EGame | null>(null);
    const [scores, setScores] = useState<{ [key: string]: number }>({});
    const [selectedAttack, setSelectedAttack] = useState<string>("regular"); // Track selected attack type
    let thisGameMode = "";
    const { connection } = useContext(SignalRContext)!;
    const currentPlayer = players.find(player => player.id === currentPlayerId);
    const currentTeam = currentPlayer?.team;
    const handlePause = async () => {
        try {
            await connection?.invoke("PauseGame", "game-1");
            document.getElementById("pauseButton")!.style.display = "none";
            document.getElementById("resumeButton")!.style.display = "inline";
        } catch (error) {
            console.error("Error pausing the game:", error);
        }
    };
    
    const handleResume = async () => {
        try {
            await connection?.invoke("ResumeGame", "game-1");
            document.getElementById("resumeButton")!.style.display = "none";
            document.getElementById("pauseButton")!.style.display = "inline";
        } catch (error) {
            console.error("Error resuming the game:", error);
        }
    };
    useEffect(() => {
        connection?.on("GamePaused", (game: EGame) => {
            setGame(game);
            setIsCurrentTurn(false);
            console.log("Game is paused.");
        });
    
        connection?.on("GameResumed", (game: EGame) => {
            setGame(game);
            setIsCurrentTurn(game.currentTurn === currentTeam);
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
            const { playerId, affectedCoordinates, result } = moveResult;
        
            // This is where you handle multiple affected cells
            for (let i = 0; i < affectedCoordinates.length; i++) {
                const { row, col } = affectedCoordinates[i];
                const currentResult = result[i]; // Each result corresponds to a specific cell
                updateBoardCell(row, col, currentResult);
            }
        });
        
        connection?.on("ReceiveGameMode", function (gameMode) {
            // Here, you can update the frontend UI with the received game mode.
            console.log("Game Mode received: ", gameMode);
            updateGameModeUI(gameMode);  // Update your UI accordingly
        });
        
    
        // Function to update UI based on the received game mode
        function updateGameModeUI(gameMode: string) {
            thisGameMode = gameMode
        }

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

    function updateBoardCell(row: number, col: number, result: "Hit" | "Miss" | "Sunk") {
        const cell = document.getElementById(`cell-${row}-${col}`);
        if (result === "Hit") {
            cell?.classList.add("hit");
        } else if (result === "Sunk") {
            cell?.classList.add("sunk");
        } else if (result === "Miss") {
            cell?.classList.add("miss");
        }
    }
    

    // Updated handleShoot function to include attack type
    const handleShoot = async (row: number, col: number, targetPlayerId: string) => {
        if (!isCurrentTurn || !currentPlayerId || targetPlayerId === currentPlayerId) return;
        
        try {
            // Send shot request with attack type (result might be an array for multiple hits)
            const result = await connection?.invoke("MakeMove", "game-1", currentPlayerId, row, col, selectedAttack);
        
            // Assuming result is an array or object containing information about the hit/miss for each square
            if (result) {
                console.log(`${currentPlayerId} shot at (${row}, ${col}): ${result}`);
                // Update game state if needed, to reflect missed shot or multiple hits
                setGame((prevGame) => {
                    if (!prevGame || !prevGame.players) return prevGame;
        
                    const updatedGame = { ...prevGame };
                    const targetPlayer = updatedGame.players[targetPlayerId];
        
                    if (targetPlayer) {
                        if (Array.isArray(result)) {
                            result.forEach(({ row, col, hit }) => {
                                targetPlayer.board.grid[row][col].isHit = hit === 'hit';
                                targetPlayer.board.grid[row][col].isMiss = hit === 'miss';
                            });
                        } else {
                            targetPlayer.board.grid[result.row][result.col].isHit = result.hit === 'hit';
                            targetPlayer.board.grid[result.row][result.col].isMiss = result.hit === 'miss';
                        }
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
        if (game && game.players) {
            const playerInGame = game.players[playerId];
            return playerInGame ? playerInGame.board : null;
        }
        return null; 
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
            <h2>Game Board for {game?.mode} mode</h2>
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

            <div className="boards-container">
                <div className="board-row">
                    {currentPlayer && (
                        <>
                            <div key={currentPlayer.id} className="board-wrapper" style={{backgroundColor: getTeamColor(currentPlayer.team)}}>
                                <div className="board-title">{currentPlayer.name}'s Board</div>
                                {game && game.state === "Paused" && (
                                    <p>The game is currently paused.</p>
                                )}
                                <button id="pauseButton" onClick={handlePause}>Pause Game</button>
                                <button id="resumeButton" style={{ display: "none" }} onClick={handleResume}>Resume Game</button>
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
        </div>
    );
};

export default GameBoard;
