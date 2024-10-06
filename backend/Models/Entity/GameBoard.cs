using backend.Models.Entity.Bombs;
using backend.Models.Entity.GameBoardExtensions;
using backend.Models.Entity.Iterator;
using backend.Models.Entity.Ships;
using backend.Strategies;
using Shared;


namespace backend.Models.Entity;

//Would be better to write a separate class for the grid
public class GameBoard
{
    private ShipCollection _battleships = new ShipCollection();
    private HashSet<ShipCoordinate> _missedCoordinates = new();
    private Ship? _enemyAttackShip;
    private ThemeAbstraction _theme;

    public void SetTheme(ThemeAbstraction theme)
    {
        _theme = theme;
    }
    public ThemeAbstraction GetTheme() => _theme;

    public int maxSizeX { get { return 10; } }
    public int maxSizeY { get { return 10; } }

    public GameBoardMomento CreateMomento()
    {
        return new GameBoardMomento(GetClonedShipsCollection(), new HashSet<ShipCoordinate>(_missedCoordinates), _enemyAttackShip, _theme);
    }
    public void RestoreFromMomento(GameBoardMomento momento)
    {
        _battleships = momento.Battleships;
        _missedCoordinates = momento.MissedCoordinates;
        _enemyAttackShip = momento.EnemyAttackShip;
        _theme = momento.Theme;
    }
    public void SetEnemyAttackShip(Ship ship)
    {
        _enemyAttackShip = ship;
    }
    public Ship GetEnemyAttackShip()
    {
        return _enemyAttackShip ?? throw new NullReferenceException("Enemy ship doesnt exist");
    }
    public int GetStandingShipCount()
    {
        return GetShips().Where(s => !s.IsSunk()).ToList().Count;
    }
    public void AddMissed(ShipCoordinate coordinate)
    {
        _missedCoordinates.Add(coordinate);
    }
    public void RemoveMissed(ShipCoordinate coordinate)
    {
        _missedCoordinates.Remove(coordinate);
    }
    public void AddShip(Ship ship)
    {
        ship.ID = _battleships.getItems().Count;
        _battleships.AddItem(ship);
    }
    public Ship GetHitShip(ShipCoordinate coordinate)
    {
        foreach(var hitShip in _battleships.getItems())
        {
            if (hitShip.GetCoordinates().Contains(coordinate))
                return hitShip;
        }
        return null;
    }
    public void ReplaceShipAt(int index, Ship ship)
    {
        if (index >= _battleships.getItems().Count)
        {
            return;
        }
        _battleships.getItems()[index] = ship;
    }
    public bool HaveAllShipsSunk
    {
        get
        {
            return _battleships.getItems().All(x => x.IsSunk());
        }
    }
    public List<ShipCoordinate> GetMissedCoordinates()
    {
        return new List<ShipCoordinate>(_missedCoordinates);
    }
    public void ClearMissedCoordinates()
    {
        _missedCoordinates = new();
    }
    private ShipCollection GetClonedShipsCollection()
    {
        List<Ship> clonedShips =  new List<Ship>(_battleships.getItems().Select(s => s.DeepCopy()));
        ShipCollection clonedShipCollection = new ShipCollection();
        foreach (var ship in clonedShips)
        {
            clonedShipCollection.AddItem(ship);
        }
        return clonedShipCollection;
    }

    public List<Ship> GetShips()
    {
        return new List<Ship>(_battleships.getItems());
    }

}