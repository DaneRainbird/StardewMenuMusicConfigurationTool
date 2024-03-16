using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewMenuMusicConfigurationTool {
	internal sealed class ModEntry : Mod {

        /// <summary>
        /// The mod configuration as set by the player
        /// </summary>
		private ModConfig Config;

		/// <summary>
		/// Entry point function of the mod
		/// </summary>
		/// <param name="helper"></param>
		public override void Entry(IModHelper helper) {
			this.Config = this.Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += onGameLaunched;
			helper.Events.GameLoop.ReturnedToTitle += handleReturnToTitle;
        }

        /// <summary>
        /// The game launch event handler, responsible for setting up the mod menu and adjusting volumes as per config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void onGameLaunched(object sender, GameLaunchedEventArgs e) {
			// Handle setting the menu volumes
			getAndSetMenuVolumes();

			// Configure the Generic Mod Config Menu
			configureGenericGameMenu();
        }

		/// <summary>
		/// Configures the Generic Mod Config Menu by adding all value sliders and callbacks.
		/// </summary>
		public void configureGenericGameMenu() {
			var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			configMenu.Register(
				mod: this.ModManifest,
				reset: () => this.Config = new ModConfig(),
				save: () => this.Helper.WriteConfig(this.Config),
				titleScreenOnly: true
			);

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Menu Music Volume",
				getValue: () => this.Config.gameLaunchMusicVolume,
				setValue: value => this.Config.gameLaunchMusicVolume = (int)value,
				min: 0,
				max: 100,
				interval: 1,
				tooltip: () => "Changes the background music volume.",
				fieldId: "menuMusicVolumeField"
			);

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Menu Sound Volume",
				getValue: () => this.Config.gameLaunchSoundVolume,
				setValue: value => this.Config.gameLaunchSoundVolume = (int)value,
				min: 0,
				max: 100,
				interval: 1,
				tooltip: () => "Changes the sound volume (e.g. button clicks, etc.).",
				fieldId: "menuSoundVolumeField"
			);

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Menu Ambient Volume",
				getValue: () => this.Config.gameLaunchAmbientVolume,
				setValue: value => this.Config.gameLaunchAmbientVolume = (int)value,
				min: 0,
				max: 100,
				interval: 1,
				tooltip: () => "Changes the ambient volume (e.g. birds chirping, wind howling, etc.).",
				fieldId: "menuSoundAmbientField"
			);

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Menu Footstep Volume",
				getValue: () => this.Config.gameLaunchFootstepVolume,
				setValue: value => this.Config.gameLaunchFootstepVolume = (int)value,
				min: 0,
				max: 100,
				interval: 1,
				tooltip: () => "Changes the footstep volume (plays when hovering over menu items).",
				fieldId: "menuSoundFootstepField"
			);

			// Callback for the fieldValue changing.
			configMenu.OnFieldChanged(
				mod: this.ModManifest,
				onChange: (fieldId, newValue) => {
					switch (fieldId) {
						case "menuMusicVolumeField":
							Game1.options.musicVolumeLevel = validateVolume((int)newValue);
							Game1.musicCategory.SetVolume(validateVolume((int)newValue));
							break;
						case "menuSoundVolumeField":
							Game1.options.soundVolumeLevel = validateVolume((int)newValue);
							Game1.soundCategory.SetVolume(validateVolume((int)newValue));
							break;
						case "menuSoundAmbientField":
							Game1.options.ambientVolumeLevel = validateVolume((int)newValue);
							Game1.ambientCategory.SetVolume(validateVolume((int)newValue));
							break;
						case "menuSoundFootstepField":
							Game1.options.footstepVolumeLevel = validateVolume((int)newValue);
							Game1.footstepCategory.SetVolume(validateVolume((int)newValue));
							break;
						default:
							log($"Unexpected field change id {fieldId}", LogLevel.Error);
							break;
					}
				}
			);
		}

		/// <summary>
		/// Helper functions to set volumes for the menu
		/// </summary>
		/// <param name="volumes">An array of volume values, in the order of music, sound, ambient, footstep.</param>
		public void setMenuVolumes(float[] volumes) {
			// Set music
			Game1.options.musicVolumeLevel = volumes[0];
			Game1.musicCategory.SetVolume(volumes[0]);

			// Set sounds 
			Game1.options.soundVolumeLevel = volumes[1];
			Game1.soundCategory.SetVolume(volumes[1]);

			// Set ambient
			Game1.options.ambientVolumeLevel = volumes[2];
			Game1.ambientCategory.SetVolume(volumes[2]);

			// Set footsteps 
			Game1.options.footstepVolumeLevel = volumes[3];
			Game1.footstepCategory.SetVolume(volumes[3]);
		}

		/// <summary>
		/// Helper function that fires upon return to the menu from the game
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void handleReturnToTitle(object sender, ReturnedToTitleEventArgs e) {
			getAndSetMenuVolumes();
		}

		/// <summary>
		/// Helper function to get and set the volumes from the config file
		/// </summary>
		public void getAndSetMenuVolumes() {
			// Get the volumes from the config and validate 
			float[] volumes = {
				validateVolume(this.Config.gameLaunchMusicVolume),
				validateVolume(this.Config.gameLaunchSoundVolume),
				validateVolume(this.Config.gameLaunchAmbientVolume),
				validateVolume(this.Config.gameLaunchFootstepVolume)
			};

			// Set the menu volumes on bulk
			setMenuVolumes(volumes);
		}

		/// <summary>
		/// Helper function to simplify logging to SMAPI standard out.
		/// </summary>
		/// <param name="message">The message to be logged.</param>
		/// <param name="logLevel">The LogLevel of the log.</param>
		public void log(string message, LogLevel logLevel) {
			this.Monitor.Log(message, logLevel);
		}

		/// <summary>
		/// Helper function to validate that the provided volume is valid
		/// </summary>
		/// <param name="volume">The integer volume value as provided by config.</param>
		/// <returns>A normalised float between 0 and 1.</returns>
		public float validateVolume(int volume) {
			if (volume > 100 || volume < 0) {
				this.Monitor.Log($"Invalid volume of {volume} provided (either <0 or >100). Defaulting to 100", LogLevel.Error);
				return 1;
			}

			// Return the float value between 0-1 as Stardew only handles volumes between those values
			return (float)volume / 100f;
		}
	}
}