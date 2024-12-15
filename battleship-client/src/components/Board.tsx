import React, { useEffect, useState, useContext } from "react";
import { Cell, Ship } from "../models"; // Ensure proper imports
import { SignalRContext } from "../contexts/SignalRContext";
import "../style/GameBoard.css";

interface BoardProps {
  board: Cell[][]; // 2D array representing the board
  isPlayerBoard: boolean; // Indicates if this board is the player's
  isTeammateBoard?: boolean; // Indicates if the board belongs to a teammate
  onShipsPlaced?: () => void; // Callback when ships are placed
  onShoot?: (row: number, col: number) => Promise<void>; // Allows shooting at enemy boards
  playerName: string; // Player's name to display on the board
  playerId: string; // Player's ID for SignalR
  gameId: string; // Game ID to identify the current game session
  score?: number; //the score for that player
  team?: string; //color of the team
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
  score,
  team,
}) => {
  const [localBoard, setLocalBoard] = useState<Cell[][]>(board || [[]]);
  const [selectedShip, setSelectedShip] = useState<Ship | null>(null);
  const [ships, setShips] = useState<Ship[]>([
    {
      name: "Destroyer",
      length: 2,
      coordinates: [],
      isPlaced: false,
      orientation: "horizontal",
      hitCount: 0,
      isSunk: false,
    },
    {
      name: "Submarine",
      length: 3,
      coordinates: [],
      isPlaced: false,
      orientation: "horizontal",
      hitCount: 0,
      isSunk: false,
    },
    {
      name: "Cruiser",
      length: 3,
      coordinates: [],
      isPlaced: false,
      orientation: "horizontal",
      hitCount: 0,
      isSunk: false,
    },
    {
      name: "Battleship",
      length: 4,
      coordinates: [],
      isPlaced: false,
      orientation: "horizontal",
      hitCount: 0,
      isSunk: false,
    },
    {
      name: "Carrier",
      length: 5,
      coordinates: [],
      isPlaced: false,
      orientation: "horizontal",
      hitCount: 0,
      isSunk: false,
    },
  ]);
  const [placedShips, setPlacedShips] = useState<Ship[]>([]);
  const { setPlayerReady } = useContext(SignalRContext)!;
  const { connection } = useContext(SignalRContext)!;
  const [isReady, setIsReady] = useState(false); // State to track if the "Ready" button is clicked

  useEffect(() => {
    setLocalBoard(board);
  }, [board]);

  useEffect(() => {
    connection?.on("ShipPlaced", (updatedBoard) => {
      setLocalBoard(updatedBoard.grid); // Update the local board state
      setPlacedShips(updatedBoard.ships); // Update the placed ships
      setShips((prevShips) =>
        prevShips.map((ship) =>
          updatedBoard.ships.some((s: Ship) => s.name === ship.name)
            ? { ...ship, isPlaced: true }
            : ship
        )
      );
    });

    connection?.on("ShipPlacementUndone", (updatedBoard) => {
      setLocalBoard(updatedBoard.grid);
      setPlacedShips(updatedBoard.ships);
      // Reset the 'isPlaced' status in the ships array
      setShips((prevShips) =>
        prevShips.map(
          (ship) =>
            // If the ship is in placedShips but not on updatedBoard.ships, it was just undone
            updatedBoard.ships.some((s: Ship) => s.name === ship.name)
              ? ship
              : { ...ship, isPlaced: false } // Mark as not placed for the undone ship
        )
      );
    });

    connection?.on("ShipPlacementFailed", (errorMessage) => {
      console.error("Ship placement failed:", errorMessage);
      alert(`Ship placement failed: ${errorMessage}`);
    });
  }, [connection]);

  const handleCellClick = async (row: number, col: number) => {
    if (isPlayerBoard && selectedShip) {
      try {
        await connection?.invoke(
          "PlaceShip",
          gameId,
          playerId,
          selectedShip.name,
          row,
          col,
          selectedShip.orientation
        );
        console.log(
          `Placement request sent for ${selectedShip.name} at (${row}, ${col}) with orientation ${selectedShip.orientation}`
        );
        setSelectedShip(null);
      } catch (error) {
        console.error("Error placing ship:", error);
      }
    } else if (!isPlayerBoard && !isTeammateBoard && onShoot) {
      onShoot(row, col);
    }
  };

  const handleReadyClick = async () => {
    setPlayerReady(gameId, playerId); // Signal that the player is ready
    setIsReady(true); // Hide the "Ready" button
  };

  const handleRandomShips = async () => {
    try {
      await connection?.invoke("GenerateRandomShips", gameId, playerId);
      setSelectedShip(null);
    } catch (error) {
      console.error("Error generating random ships:", error);
    }
  };

  const handleUndoPlacement = async () => {
    if (placedShips.length !== 0) {
      try {
        await connection?.invoke("UndoLastPlacement", gameId, playerId);
        console.log("Undo placement request sent");
      } catch (error) {
        console.error("Error undoing last placement:", error);
      }
    }
  };

  return (
    <div>
      {isPlayerBoard && (
        <div>
          <div>
            {ships.map(
              (ship) =>
                !ship.isPlaced && (
                  <button key={ship.name} onClick={() => setSelectedShip(ship)}>
                    {ship.name} ({ship.length})
                  </button>
                )
            )}
          </div>
          {selectedShip && (
            <div>
              <button
                onClick={() =>
                  setSelectedShip({
                    ...selectedShip,
                    orientation:
                      selectedShip.orientation === "horizontal"
                        ? "vertical"
                        : "horizontal",
                  })
                }
              >
                Change Orientation: {selectedShip.orientation}
              </button>
            </div>
          )}
          {!isReady && (
            <div>
              <button
                onClick={handleUndoPlacement}
                style={{ marginLeft: "10px" }}
              >
                Undo Last Placement
              </button>
              <button onClick={handleRandomShips}>Generate random ships</button>
            </div>
          )}
        </div>
      )}
      <h3>
        {playerName}'s Board {isTeammateBoard && "(Teammate)"}
      </h3>
      <p>Score: {score}</p> {/* Display the score here */}
      <div
        className="board-grid"
        style={{
          display: "grid",
          gridTemplateColumns: `repeat(${localBoard[0].length}, 30px)`,
          gap: "2px",
        }}
      >
        {localBoard.map((row, rowIndex) =>
          row.map((cell, colIndex) => (
            <div
              key={`${playerId}-${rowIndex}-${colIndex}`}
              id={`cell-${playerId}-${rowIndex}-${colIndex}`}
              onClick={() => handleCellClick(rowIndex, colIndex)}
              className={
                cell.isHit
                  ? "isHit"
                  : cell.isMiss
                  ? "isMissed" // New class for missed shots
                  : cell.hasShip
                  ? isPlayerBoard || isTeammateBoard
                    ? "hasShip"
                    : "empty"
                  : "empty"
              }
              style={{
                width: "30px",
                height: "30px",
                backgroundColor: cell.isHit
                  ? "red"
                  : cell.isMiss
                  ? "#a9a9a9" // Light gray for missed shots
                  : cell.hasShip
                  ? isPlayerBoard || isTeammateBoard
                    ? "blue"
                    : "white"
                  : "white",
                border: "1px solid black",
                cursor:
                  !isPlayerBoard && !isTeammateBoard && onShoot
                    ? "pointer"
                    : "default",
              }}
            />
          ))
        )}
      </div>
      {isPlayerBoard && placedShips.length === ships.length && !isReady && (
        <button onClick={handleReadyClick} style={{ marginTop: "10px" }}>
          Ready
        </button>
      )}
    </div>
  );
};

export default Board;
