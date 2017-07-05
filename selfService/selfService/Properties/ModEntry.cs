﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
// using System.Threading.Tasks;

namespace SelfServe
{
    public class ModEntry : Mod
    {
        private List<Vector2> seedShopCounterTiles = new List<Vector2>();
        private List<Vector2> animalShopCounterTiles = new List<Vector2>();
        private List<Vector2> CarpentersShopCounterTiles = new List<Vector2>();

        private bool inited = false;

        private void Bootstrap(object Sender, EventArgs e)
        {
            seedShopCounterTiles.Add(new Vector2(4f, 19f));
            seedShopCounterTiles.Add(new Vector2(5f, 19f));

            animalShopCounterTiles.Add(new Vector2(12f, 16f));
            animalShopCounterTiles.Add(new Vector2(13f, 16f));

            CarpentersShopCounterTiles.Add(new Vector2(8f, 20f));

            this.inited = true;
        }

        public override void Entry(IModHelper helper)
        {
            ControlEvents.KeyPressed += this.KeyEventHandler;
            ControlEvents.ControllerButtonPressed += this.ControllerEventHandler;
            // ControlEvents.MouseChanged += this.mouseEventHandler; // this would be too ugly to implement with current version of SMAPI
            SaveEvents.AfterLoad += this.Bootstrap;
        }

        private void KeyEventHandler(object sender, EventArgsKeyPressed e)
        {   
            if (inited && OpenMenuHandler(Array.Exists(Game1.options.actionButton, item => e.KeyPressed.Equals(item.key))))
            {
                Game1.oldKBState = Keyboard.GetState();
            }
        }

        private void ControllerEventHandler(object sender, EventArgsControllerButtonPressed e)
        {
            // NOTE: looks like the game has hard coded  button to key mappings, isActionKey() is subject to change if customized key mapipng is allowed in the future
            // See code below:
            //public static Keys mapGamePadButtonToKey(Buttons b)
            //{
            //    if (b == Buttons.A)
            //        return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.actionButton);

            if (inited && OpenMenuHandler(e.ButtonPressed.Equals(Buttons.A)))
            {
                Game1.oldPadState = GamePad.GetState(PlayerIndex.One);
            }
        }

        private bool OpenMenuHandler(bool isActionKey)
        {
            // returns true if menu is opened, otherwise false

            String locationString = Game1.player.currentLocation.name;
            Vector2 playerPosition = Game1.player.getTileLocation();
            int faceDirection = Game1.player.getFacingDirection();

            bool result = false; // default

            if (ShouldOpen(isActionKey, Game1.player.getFacingDirection(), locationString, playerPosition))
            {
                // NOTE: the game won't set dialogue if there's an active menu, so no more warpping magics lol

                result = true;
                switch (locationString)
                {
                    case "SeedShop":
                        Game1.activeClickableMenu = (IClickableMenu)new ShopMenu(Utility.getShopStock(true), 0, "Pierre");
                        break;
                    case "AnimalShop":
                        Game1.player.currentLocation.createQuestionDialogue(
                            "",
                            new Response[3]
                            {
                                new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies")),
                                new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals")),
                                new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave"))
                            },
                            "Marnie"
                        );
                        break;
                    case "ScienceHouse":
                        if (Game1.player.daysUntilHouseUpgrade < 0 && !Game1.getFarm().isThereABuildingUnderConstruction())
                        {
                            Response[] answerChoices;
                            if (Game1.player.houseUpgradeLevel < 3)
                                answerChoices = new Response[4]
                                {
                                    new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")),
                                    new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeHouse")),
                                    new Response("Construct", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct")),
                                    new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave"))
                                };
                            else
                                answerChoices = new Response[3]
                                {
                                    new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")),
                                    new Response("Construct", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct")),
                                    new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave"))
                                };

                            Game1.player.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu"), answerChoices, "carpenter");
                        }
                        else
                        {
                            Game1.activeClickableMenu = (IClickableMenu)new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
                        }
                        break;
                    default:
                        Monitor.Log($"invalid location: {locationString}", LogLevel.Info);
                        break;
                }
            }

            return result;

        }

        private bool ShouldOpen(bool isActionKey, int facingDirection, String locationString, Vector2 playerLocation)
        {
            // Monitor.Log($"{locationString} {playerLocation.ToString()}");
            if (Game1.activeClickableMenu == null && isActionKey && facingDirection == 3) // somehow SMAPI doesn't provide enum for facing directions?
            {
                switch (locationString)
                {
                    case "SeedShop":
                        return this.seedShopCounterTiles.Contains(playerLocation);
                    case "AnimalShop":
                        return this.animalShopCounterTiles.Contains(playerLocation);
                    case "ScienceHouse":
                        return this.CarpentersShopCounterTiles.Contains(playerLocation);
                    default:
                        return false;
                }
            }

            return false; // default
        }
    }
}