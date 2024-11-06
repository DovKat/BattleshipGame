import React, { useEffect, useState, useContext } from 'react';
import { Cell, Ship, Coordinate, ShipE } from '../models'; // Ensure proper imports
import { SignalRContext } from '../contexts/SignalRContext';
import '../style/GameBoard.css';

interface BoardProps {
    board: Cell[][]; // 2D array representing the board
    isPlayerBoard: boolean; // Indicates if this board is the player's
    isTeammateBoard?: boolean; // Indicates if the board belongs to a teammate
    onShipsPlaced?: () => void; // Callback when ships are placed
    onShoot?: (row: number, col: number) => Promise<void>; // Allows shooting at enemy boards
    playerName: string; // Player's name to display on the board
    playerId: string; // Player's ID for SignalR
    gameId: string; // Game ID to identify the current game session
    score?: number //the score for that player
}

const Board: React.FC<BoardProps> = ({
    board,
    isPlayerBoard,
    isTeammateBoard = false,
    onShipsPlaced,
    onShoot,
    playerName,
    playerId,
    gameId,
    score
}) => {
    const [localBoard, setLocalBoard] = useState<Cell[][]>(board);
    const [selectedShip, setSelectedShip] = useState<Ship | null>(null);
    const [ships, setShips] = useState<Ship[]>([
        { name: 'Destroyer', length: 2, coordinates: [], isPlaced: false,orientation: 'horizontal',hitCount: 0 ,isSunk: false},
        { name: 'Submarine', length: 3, coordinates: [], isPlaced: false,orientation: 'horizontal',hitCount: 0,isSunk: false },
        { name: 'Cruiser', length: 3, coordinates: [], isPlaced: false,orientation: 'horizontal',hitCount: 0,isSunk: false },
        { name: 'Battleship', length: 4, coordinates: [], isPlaced: false ,orientation: 'horizontal',hitCount: 0,isSunk: false},
        { name: 'Carrier', length: 5, coordinates: [], isPlaced: false,orientation: 'horizontal',hitCount: 0,isSunk: false},
    ]);
    const [placedShips, setPlacedShips] = useState<Ship[]>([]);
    const { setPlayerReady } = useContext(SignalRContext)!;
    const { connection } = useContext(SignalRContext)!;
    useEffect(() => {
        setLocalBoard(board);
    }, [board]);

    const canPlaceShip = (row: number, col: number, orientation: 'horizontal' | 'vertical', length: number) => {

        return true; 
    };

    const handleCellClick = (row: number, col: number) => {
        if (isPlayerBoard && selectedShip) {
            if (canPlaceShip(row, col, selectedShip.orientation, selectedShip.length)) {
                const newBoard = [...localBoard];
                const shipCoordinates: Coordinate[] = []; 
    
                for (let i = 0; i < selectedShip.length; i++) {
                    const r = selectedShip.orientation === 'horizontal' ? row : row + i;
                    const c = selectedShip.orientation === 'horizontal' ? col + i : col;
    

                    if (r < newBoard.length && c < newBoard[r].length) {
                        newBoard[r][c].hasShip = true; 
                        shipCoordinates.push({ row: r, column: c }); 
                    } else {
                        console.error(`Attempted to place ship out of bounds at (${r}, ${c})`);
                        return; 
                    }
                }
    
                const updatedShip: Ship = {
                    name: selectedShip.name,
                    length: selectedShip.length,
                    orientation: selectedShip.orientation,
                    coordinates: shipCoordinates,
                    hitCount: 0, 
                    isSunk: false,
                    isPlaced: true
                };
    

                setLocalBoard(newBoard);
                setPlacedShips(prevShips => [...prevShips, updatedShip]); 
                setShips(prevShips => 
                    prevShips.map(ship => 
                        ship.name === selectedShip.name ? { ...ship, isPlaced: true, coordinates: shipCoordinates, hitCount: 0, isSunk: false } : ship
                    )
                );
    
                if (onShipsPlaced) onShipsPlaced(); 
                setSelectedShip(null); 
            }
        } else if (!isPlayerBoard && !isTeammateBoard && onShoot) {
            onShoot(row, col);
        }
    };
    
    const handleReadyClick = async () => {
        if (placedShips.length === ships.length) { // Check if all ships are placed
            // Prepare the data to send to the server
            const allShipsData = placedShips.map(ship => ({
                name: ship.name,
                length: ship.length,
                orientation: ship.orientation,
                coordinates: ship.coordinates,
                hitCount: ship.hitCount,
                isSunk: ship.isSunk,
                isPlaced: ship.isPlaced
            }));
    
            try {
                // Send the ships data to the server
                await connection?.invoke("PlaceShip", gameId, playerId, allShipsData);
                console.log("All ships have been placed successfully.");
                setPlayerReady(gameId, playerId); // Mark the player as ready after placing ships
            } catch (error) {
                console.error("Error placing ships:", error);
            }
        }
    };
    

    return (
        <div>
            {isPlayerBoard && (
                <div>
                    <div>
                        {ships.map((ship) =>
                            !ship.isPlaced && (
                                <button key={ship.name} onClick={() => setSelectedShip(ship)}>
                                    {ship.name} ({ship.length})
                                </button>
                            )
                        )}
                    </div>
                    {selectedShip && (
                        <div>
                            <button onClick={() => setSelectedShip({ ...selectedShip, orientation: selectedShip.orientation === 'horizontal' ? 'vertical' : 'horizontal' })}>
                                Change Orientation: {selectedShip.orientation}
                            </button>
                            <button onClick={() => {
                            }} style={{ marginLeft: '10px' }}>
                                Undo Last Placement
                            </button>
                        </div>
                    )}
                </div>
            )}
            <h3>{playerName}'s Board {isTeammateBoard && "(Teammate)"}</h3>
            <p>Score: {score}</p> {/* Display the score here */}
            <div
                className="board-grid"
                style={{
                    display: 'grid',
                    gridTemplateColumns: `repeat(${localBoard[0].length}, 30px)`,
                    gap: '2px'
                }}
            >
                {localBoard.map((row, rowIndex) =>
                    row.map((cell, colIndex) => (
                        <div
                            key={`${rowIndex}-${colIndex}`}
                            onClick={() => handleCellClick(rowIndex, colIndex)}
                            className={cell.isHit ? 'isHit' :
                                       cell.hasShip ? (isPlayerBoard || isTeammateBoard ? 'hasShip' : 'empty') :
                                       'empty'}
                            style={{
                                width: '30px',
                                height: '30px',
                                backgroundColor: cell.isHit ? 'red' :
                                                 cell.hasShip ? (isPlayerBoard || isTeammateBoard ? 'blue' : 'white') :
                                                 'white',
                                border: '1px solid black',
                                cursor: !isPlayerBoard && !isTeammateBoard && onShoot ? 'pointer' : 'default'
                            }}
                        />
                    ))
                )}
            </div>
            {isPlayerBoard && placedShips.length === ships.length && (
                <button onClick={handleReadyClick} style={{ marginTop: '10px' }}>
                    Ready
                </button>
            )}
        </div>
    );
};

export default Board;
