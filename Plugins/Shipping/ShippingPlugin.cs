﻿using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;

namespace Lomztein.Moduthulhu.Modules.Shipping {

    [Descriptor ("Lomztein", "Shipping Simulator 2018", "At the core of all this lies Guffe.")]
    public class ShippingPlugin : PluginBase {

        public CachedValue<List<Ship>> _ships;
        public CachedValue<List<ShipName>> _shipNames;

        private ShippingCommands commands;

        public override void Initialize() {
            commands = new ShippingCommands () { ParentPlugin = this };
            SendMessage("Lomztein-Command Root", "AddCommand", commands);

            _ships = GetDataCache("Ships", x => new List<Ship>());
            _shipNames = GetDataCache("ShipNames", x => new List<ShipName>());
        }

        public Ship Ship (SocketGuildUser shipper, SocketGuildUser shippieOne, SocketGuildUser shippieTwo, out bool succesful) {

            Ship ship = new Ship (this, shipper.Guild.Id, shipper.Id, shippieOne.Id, shippieTwo.Id);
            succesful = false;

            if (!ContainsShip (ship)) {
                AddShip (shipper.Guild.Id, ship);
                succesful = true;
            }

            return ship;
        }

        public Ship Sink(SocketGuildUser shipper, SocketGuildUser shippieOne, SocketGuildUser shippieTwo, out bool succesful) {
            Ship ship = new Ship (this, shipper.Guild.Id, shipper.Id, shippieOne.Id, shippieTwo.Id);
            succesful = false;

            if (ContainsShip (ship)) {
                RemoveShip (shipper.Guild.Id, ship);
                succesful = true;
            }

            return ship;
        }

        public List<Ship> GetAllShipperShips(SocketGuildUser shipper)
        {
            return _ships.GetValue().Where(x => x.Shipper == shipper.Id).ToList();
        }

        public Dictionary<ulong, List<Ship>> GetShipLeaderboard(SocketGuild guild)
        {

            Dictionary<ulong, List<Ship>> leaderboard = new Dictionary<ulong, List<Ship>>();
            foreach (Ship ship in _ships.GetValue())
            {

                if (leaderboard.ContainsKey(ship.ShippieOne))
                {
                    if (!leaderboard[ship.ShippieOne].Exists(x => x.IsShippiesIdentical(ship)))
                        leaderboard[ship.ShippieOne].Add(ship);
                }
                else
                {
                    leaderboard.Add(ship.ShippieOne, new List<Ship>() { ship });
                }

                if (leaderboard.ContainsKey(ship.ShippieTwo))
                {
                    if (!leaderboard[ship.ShippieTwo].Exists(x => x.IsShippiesIdentical(ship)))
                        leaderboard[ship.ShippieTwo].Add(ship);
                }
                else
                {
                    leaderboard.Add(ship.ShippieTwo, new List<Ship>() { ship });
                }
            }

            var sortingList = leaderboard.ToList();
            sortingList.Sort((x, y) => y.Value.Count.CompareTo(x.Value.Count));
            leaderboard = sortingList.ToDictionary(x => x.Key, y => y.Value);
            return leaderboard;

        }
       

        public List<Ship> GetShippieShips(SocketGuildUser shippie) {
                return _ships.GetValue().Where (x => x.ShippieOne == shippie.Id || x.ShippieTwo == shippie.Id).ToList ();
        }

        public bool ContainsShip(Ship ship)
        {
            if (_ships.GetValue().Contains(ship))
                return true;
            return false;
        }

        public void AddShip(ulong guildId, Ship ship)
        {
            _ships.MutateValue (x => x.Add(ship));
        }

        public void RemoveShip(ulong guildId, Ship ship) {
            _ships.MutateValue (x => x.Remove (ship));
        }

        public Ship GetShipByShippies(SocketGuildUser shippieOne, SocketGuildUser shippieTwo)
        {
            return _ships.GetValue().Find(x => x.IsShippiesIdentical(shippieOne.Id, shippieTwo.Id));
        }

        public void NameShip (SocketGuildUser shippieOne, SocketGuildUser shippieTwo, string name) {
            ShipName shipName = new ShipName(shippieOne.Id, shippieTwo.Id, name);
            _shipNames.GetValue().Remove(shipName);
            _shipNames.MutateValue (x => x.Add (shipName));
        }

        public void DeleteShipName (SocketGuildUser shippieOne, SocketGuildUser shippieTwo) {
            ShipName shipName = GetCustomShipName (shippieOne.Id, shippieTwo.Id);
            if (shipName != null) {
                _shipNames.MutateValue(x => x.Remove (shipName));
            }
        }

        public ShipName GetCustomShipName (ulong shippieOne, ulong shippieTwo) {
            return _shipNames.GetValue().Find (x => x.shippieOne == shippieOne && x.shippieTwo == shippieTwo);
        }

        public string GetShipName(Ship ship)
        {
            ShipName name = GetCustomShipName(ship.ShippieOne, ship.ShippieTwo);
            if (name == null)
                return ship.GenerateContractedName(GuildHandler);
            return name.shipName;
        }

        public string ShipToString(Ship ship)
        {
            SocketGuildUser one = GuildHandler.GetUser(ship.ShippieOne);
            SocketGuildUser two = GuildHandler.GetUser(ship.ShippieTwo);

            return $"{one.GetShownName()} x {two.GetShownName()} - {GetShipName(ship)}";
        }

        public override void Shutdown() {
            SendMessage("Lomztein-Command Root", "RemoveCommand", commands);
        }

    }

    public class Ship
    {
        public ulong Shipper { get; set; }

        public ulong ShippieOne { get; set; }
        public ulong ShippieTwo { get; set; }

        public Ship(ShippingPlugin _parentModule, ulong _guildId, ulong _shipper, ulong _shippieOne, ulong _shippieTwo)
        {
            Shipper = _shipper;

            // Make sure the "orientation" of the ship is set in stone, for consistant ship names.
            ShippieOne = Math.Min(_shippieOne, _shippieTwo);
            ShippieTwo = Math.Max(_shippieOne, _shippieTwo);
        }

        public override bool Equals(object obj)
        {
            if (obj is Ship ship)
            {
                return (Shipper == ship.Shipper && IsShippiesIdentical(ship));
            }
            return false;
        }

        /// <summary>
        /// Only checks the shippies, compared to Equals which compares just about fucking everything.
        /// </summary>
        /// <param name="otherShip"></param>
        /// <returns></returns>
        public bool IsShippiesIdentical(Ship otherShip)
        {
            // Since ship orientation is consinstantly based on user ID's, comparison should be much simpler.
            return IsShippiesIdentical(otherShip.ShippieOne, otherShip.ShippieTwo);
        }

        /// <summary>
        /// Only checks the shippies, compared to Equals which compares just about fucking everything.
        /// </summary>
        /// <param name="_shippieOne"></param>
        /// <param name="_shippieTwo"></param>
        /// <returns></returns>
        public bool IsShippiesIdentical(ulong _shippieOne, ulong _shippieTwo)
        {
            // Since ship orientation is consinstantly based on user ID's, comparison should be much simpler.
            return (ShippieOne == _shippieOne && ShippieTwo == _shippieTwo);
        }

        public override int GetHashCode() => 0;



        public ulong GetCompanionTo(ulong user)
        {
            if (user == ShippieOne)
                return ShippieTwo;
            if (user == ShippieTwo)
                return ShippieOne;
            throw new ArgumentException("Given user is not a shippie in this ship.");
        }

        public string GenerateContractedName(GuildHandler nameSource)
        {
            SocketGuildUser one = nameSource.GetUser(ShippieOne);
            SocketGuildUser two = nameSource.GetUser(ShippieTwo);

            int maxLength = Math.Max(one.GetShownName().Length, two.GetShownName().Length);

            string firstPart = GetPartName(one.GetShownName(), true, maxLength);
            string lastPart = GetPartName(two.GetShownName(), false, maxLength);

            return firstPart + lastPart;
        }



        private string GetPartName(string fullName, bool firstHalf, int maxLength)
        {
            if (fullName.Length <= maxLength / 2)
            {
                return fullName;
            }

            if (firstHalf)
                return fullName.Substring(0, fullName.Length / 2);
            if (!firstHalf)
                return fullName.Substring(fullName.Length / 2);
            return fullName;
        }
    }

    public class ShipName {

        public ulong shippieOne;
        public ulong shippieTwo;

        public string shipName;

        public ShipName (ulong _shippieOne, ulong _shippieTwo, string _shipName) {

            shippieOne = Math.Min (_shippieOne, _shippieTwo);
            shippieTwo = Math.Max (_shippieOne, _shippieTwo);

            shipName = _shipName;
        }

        public override bool Equals(object obj) {
            if (obj is ShipName shipName) {
                return shippieOne == shipName.shippieOne && shippieTwo == shipName.shippieTwo;
            }
            return false;
        }

        public override int GetHashCode() => 0;

    }
}