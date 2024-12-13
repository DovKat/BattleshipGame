import React, { useEffect, useState, useContext } from 'react';
import { SignalRContext } from '../contexts/SignalRContext';
import { Game, Team, Player } from '../models';
import { v4 as uuidv4 } from 'uuid';

interface TeamSelectionProps {
    onTeamFull: (players: Player[]) => void;
    setCurrentPlayerId: (playerId: string) => void;
}

const TeamSelection: React.FC<TeamSelectionProps> = ({ onTeamFull, setCurrentPlayerId }) => {
    const signalRContext = useContext(SignalRContext);
    const [teamPlayers, setTeamPlayers] = useState<{ [key: string]: Player[] }>({ Red: [], Blue: [] });
    const [playerName, setPlayerName] = useState<string>('');
    const [selectedTeam, setSelectedTeam] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [playerId, setPlayerId] = useState<string | null>(null);
    const [gameMode, setGameMode] = useState<'standard' | 'tournament'>('standard');
    const [waitingMessage, setWaitingMessage] = useState<string>('Join a team to start!');

    useEffect(() => {
        const checkIfTeamsFull = (updatedPlayers: { [key: string]: Player[] }) => {
            if (updatedPlayers.Red.length === 2 && updatedPlayers.Blue.length === 2) {
                const playerList = [...updatedPlayers.Red, ...updatedPlayers.Blue];
                onTeamFull(playerList);
            }
        };

        if (signalRContext?.connection) {
            const connection = signalRContext.connection;

            connection.on("UpdateTeams", (updatedTeams: Team[]) => {
                const updatedPlayers: { [key: string]: Player[] } = { Red: [], Blue: [] };

                updatedTeams.forEach(team => {
                    updatedPlayers[team.name] = team.players;
                });

                setTeamPlayers(updatedPlayers);
                checkIfTeamsFull(updatedPlayers);
            });

            connection.on("GameStarted", (startedGame: Game) => {
                setWaitingMessage("Game has started!");
            });

            return () => {
                connection.off("UpdateTeams");
                connection.off("GameStarted");
            };
        }
    }, [signalRContext, onTeamFull]);

    const joinTeam = (team: string) => {
        if (signalRContext?.connection && playerName.trim()) {
            const playerId = uuidv4();
            const gameId = "game-1";

            signalRContext.connection.invoke("JoinTeam", gameId, team, playerName, playerId, gameMode)
                .then(() => {
                    setSelectedTeam(team);
                    setPlayerId(playerId);
                    setCurrentPlayerId(playerId);
                })
                .catch(err => setError("Failed to join the team. Please try again."));
        }
    };

    if (signalRContext?.isLoading) {
        return <div>Loading SignalR connection...</div>;
    }

    return (
        <div style={{ textAlign: "center", padding: "20px" }}>
            <h1>2v2 Battleship Game</h1>
            {!selectedTeam && !playerId ? (
                <div>
                    <h2>Select Game Mode</h2>
                    <select 
                        value={gameMode}
                        onChange={(e) => setGameMode(e.target.value as 'standard' | 'tournament')}
                    >
                        <option value="standard">Standard Mode</option>
                        <option value="tournament">Tournament Mode</option>
                    </select>
                    <h3>Enter Your Name</h3>
                    <input
                        type="text"
                        placeholder="Enter your name"
                        value={playerName}
                        onChange={(e) => setPlayerName(e.target.value)}
                    />
                    <div>
                        <TeamButton team="Red" playersCount={teamPlayers.Red.length} onClick={joinTeam} />
                        <TeamButton team="Blue" playersCount={teamPlayers.Blue.length} onClick={joinTeam} />
                    </div>
                    {error && <p style={{ color: 'red' }}>{error}</p>}
                </div>
            ) : (
                <TeamStatus 
                    waitingMessage={waitingMessage} 
                    teamPlayers={teamPlayers} 
                />
            )}
        </div>
    );
};

const TeamButton: React.FC<{ team: string; playersCount: number; onClick: (team: string) => void; }> = ({ team, playersCount, onClick }) => (
    <button
        onClick={() => onClick(team)}
        disabled={playersCount >= 2}
    >
        {`Join Team ${team} (${playersCount}/2)`}
    </button>
);

const TeamStatus: React.FC<{ waitingMessage: string; teamPlayers: { [key: string]: Player[] }; }> = ({ waitingMessage, teamPlayers }) => (
    <div>
        <h2>{waitingMessage}</h2>
        <div>
            <h3>Team Red</h3>
            <ul>{teamPlayers.Red.map(player => <li key={player.id}>{player.name}</li>)}</ul>
        </div>
        <div>
            <h3>Team Blue</h3>
            <ul>{teamPlayers.Blue.map(player => <li key={player.id}>{player.name}</li>)}</ul>
        </div>
    </div>
);

export default TeamSelection;
