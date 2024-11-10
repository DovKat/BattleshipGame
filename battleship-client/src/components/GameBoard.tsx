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

    const { connection } = useContext(SignalRContext)!;
    const currentPlayer = players.find(player => player.id === currentPlayerId);
    const currentTeam = currentPlayer?.team;

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

        connection?.on("MoveResult", (PlayerId, Row, Col, Result) => {
            console.log(`${PlayerId} shot at (${Row}, ${Col}): ${Result}`);
        });

        connection?.on("ReceiveUpdatedScore", (playerId: string, score: number) => {
            setScores(prevScores => ({
                ...prevScores,
                [playerId]: score
            }));
        });

        return () => {
            connection?.off("GameStarted");
            connection?.off("UpdateGameState");
            connection?.off("MoveResult");
            connection?.off("ReceiveUpdatedScore");
        };
    }, [connection, currentTeam]);

    // Updated handleShoot function to include attack type
    const handleShoot = async (row: number, col: number, targetPlayerId: string) => {
        if (!isCurrentTurn || !currentPlayerId || targetPlayerId === currentPlayerId) return;
    
        try {
            // Send shot request with attack type
            const result = await connection?.invoke("MakeMove", "game-1", currentPlayerId, row, col, selectedAttack);
    
            // Assuming result is an object that contains the status (hit/miss)
            if (result) {
                console.log(`${currentPlayerId} shot at (${row}, ${col}): ${result}`);
                // Update game state if needed, to reflect missed shot
                setGame((prevGame) => {
                    if (!prevGame || !prevGame.players) return prevGame;  // Return early if game or players is null/undefined
                    
                    const updatedGame = { ...prevGame };
                    const targetPlayer = updatedGame.players[targetPlayerId];
                    if (targetPlayer) {
                        targetPlayer.board.grid[row][col].isHit = result === 'hit';
                        targetPlayer.board.grid[row][col].isMiss = result === 'miss';
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
            <h2>Game Board</h2>
            {gameStarted && <p>The game has started!</p>}
            {isCurrentTurn ? (
                <p>It's your turn!</p>
            ) : (
                <p>Waiting for {game?.currentTurn} to play...</p>
            )}

            {/* Attack Selection Buttons for the Attacking Player */}
            {isCurrentTurn && (
                <div className="attack-buttons">
                    <button onClick={() => setSelectedAttack("regular")}>Regular Attack</button>
                    <button onClick={() => setSelectedAttack("small-bomb")}>Small Bomb</button>
                    <button onClick={() => setSelectedAttack("big-bomb")}>Big Bomb</button>
                    <button onClick={() => setSelectedAttack("super-bomb")}>Super Bomb</button>
                    <p>Selected Attack: {selectedAttack}</p>
                </div>
            )}

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
        </div>
    );
};

export default GameBoard;
