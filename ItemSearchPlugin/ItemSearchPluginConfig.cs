﻿using CheapLoc;
using Dalamud.Configuration;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud;

namespace ItemSearchPlugin {
    public class ItemSearchPluginConfig : IPluginConfiguration {
        [NonSerialized] private DalamudPluginInterface pluginInterface;
        [JsonIgnore] internal List<(string localizationKey, string englishName)> FilterNames { get; } = new List<(string localizationKey, string englishName)>();

        public int Version { get; set; }

        public string Language { get; set; }

        public bool CloseOnChoose { get; set; }

        public bool ShowItemID { get; set; }

        public bool ExtraFilters { get; set; }

        public uint MaxItemLevel { get; set; }

        public bool ShowTryOn { get; set; }

        public string DataSite { get; set; }

        public List<FittingRoomSave> FittingRoomSaves { get; set; }

        public bool MarketBoardPluginIntegration { get; set; }

        public bool EnableFittingRoomSaves { get; set; }

        public bool ShowLegacyItems { get; set; }

        public byte SelectedLanguage { get; set; }

        public List<string> DisabledFilters { get; set; }
        
        [NonSerialized] private DataSite lastDataSite = null;

        [JsonIgnore]
        public DataSite SelectedDataSite {
            get {
                if (lastDataSite == null || (lastDataSite.Name != this.DataSite)) {
                    if (string.IsNullOrEmpty(this.DataSite)) {
                        return null;
                    }

                    lastDataSite = ItemSearchPlugin.DataSites.FirstOrDefault(ds => ds.Name == this.DataSite);
                }

                return lastDataSite;
            }
        }

        [JsonIgnore]
        public ClientLanguage SelectedClientLanguage {
            get {
                return SelectedLanguage switch {
                    0 => pluginInterface.ClientState.ClientLanguage,
                    1 => ClientLanguage.English,
                    2 => ClientLanguage.Japanese,
                    3 => ClientLanguage.French,
                    4 => ClientLanguage.German,
                    _ => pluginInterface.ClientState.ClientLanguage,
                };
            }
        }

        public ItemSearchPluginConfig() {
            LoadDefaults();
        }

        public void LoadDefaults() {
            CloseOnChoose = false;
            ShowItemID = false;
            ExtraFilters = false;
            MarketBoardPluginIntegration = false;
            MaxItemLevel = 505;
            ShowTryOn = false;
            EnableFittingRoomSaves = true;
            ShowLegacyItems = false;
            DataSite = ItemSearchPlugin.DataSites.FirstOrDefault()?.Name;
            SelectedLanguage = 0;
            DisabledFilters = new List<string>();

            if (FittingRoomSaves == null) {
                FittingRoomSaves = new List<FittingRoomSave>();
            }
        }


        public void Init(DalamudPluginInterface pluginInterface) {
            this.pluginInterface = pluginInterface;
        }

        public void Save() {
            this.pluginInterface.SavePluginConfig(this);
        }


        public bool DrawConfigUI() {
            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse;
            bool drawConfig = true;
            ImGui.Begin("Item Search Config", ref drawConfig, windowFlags);

            int selectedLanguage = SelectedLanguage;
            if (ImGui.BeginCombo(Loc.Localize("ItemSearchConfigItemLanguage", "Item Language") + "###ItemSearchConfigLanguageSelect", SelectedLanguage == 0 ? Loc.Localize("LanguageAutomatic", "Automatic") : SelectedClientLanguage.ToString())) {
                if (ImGui.Selectable(Loc.Localize("LanguageAutomatic", "Automatic"), selectedLanguage == 0)) SelectedLanguage = 0;
                if (ImGui.Selectable(Loc.Localize("LanguageEnglish", "English"), selectedLanguage == 1)) SelectedLanguage = 1;
                if (ImGui.Selectable(Loc.Localize("LanguageJapanese", "Japanese"), selectedLanguage == 2)) SelectedLanguage = 2;
                if (ImGui.Selectable(Loc.Localize("LanguageFrench", "French"), selectedLanguage == 3)) SelectedLanguage = 3;
                if (ImGui.Selectable(Loc.Localize("LanguageGerman", "German"), selectedLanguage == 4)) SelectedLanguage = 4;
                if (SelectedLanguage != selectedLanguage) {
                    Save();
                }

                ImGui.EndCombo();
            }

            bool closeOnChoose = CloseOnChoose;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigCloseAfterLink", "Close window after linking item"), ref closeOnChoose)) {
                CloseOnChoose = closeOnChoose;
                Save();
            }

            bool showItemId = ShowItemID;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigShowItemId", "Show Item IDs"), ref showItemId)) {
                ShowItemID = showItemId;
                Save();
            }

            bool extraFilters = ExtraFilters;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigExtraFilters", "Enable Extra Filters"), ref extraFilters)) {
                ExtraFilters = extraFilters;
                Save();
            }

            bool mbpIntegration = MarketBoardPluginIntegration;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigEnableMarketBoard", "Market Board Plugin Integration"), ref mbpIntegration)) {
                MarketBoardPluginIntegration = mbpIntegration;
                Save();
            }

            bool showTryOn = ShowTryOn;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigEnableTryOn", "Enable Try On Feature"), ref showTryOn)) {
                ShowTryOn = showTryOn;
                Save();
            }

            bool enableFittingRoomSaves = EnableFittingRoomSaves;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigEnableFittingRoomSaves", "Enable Outfit Saving"), ref enableFittingRoomSaves)) {
                EnableFittingRoomSaves = enableFittingRoomSaves;
                Save();
            }

            bool showLegacyItems = ShowLegacyItems;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigShowLegacyItems", "Show Legacy Items"), ref showLegacyItems)) {
                ShowLegacyItems = showLegacyItems;
                Save();
            }

            int dataSiteIndex = Array.IndexOf(ItemSearchPlugin.DataSites, this.SelectedDataSite);
            if (ImGui.Combo(Loc.Localize("ItemSearchConfigExternalDataSite", "External Data Site"), ref dataSiteIndex, ItemSearchPlugin.DataSites.Select(t => Loc.Localize(t.NameTranslationKey, t.Name) + (string.IsNullOrEmpty(t.Note) ? "" : "*")).ToArray(), ItemSearchPlugin.DataSites.Length)) {
                this.DataSite = ItemSearchPlugin.DataSites[dataSiteIndex].Name;
                Save();
            }

            if (!string.IsNullOrEmpty(SelectedDataSite.Note)) {
                ImGui.TextColored(new Vector4(1, 1, 1, 0.5f), $"*{SelectedDataSite.Note}");
            }

            ImGui.Text("Show Filters: ");

            ImGui.BeginChild("###scrollingFilterSelection", new Vector2(0, 180), true);

            foreach (var (localizationKey, englishName) in FilterNames) {
                var enabled = !DisabledFilters.Contains(localizationKey);
                if (ImGui.Checkbox(Loc.Localize(localizationKey, englishName) + "##checkboxToggleFilterEnabled", ref enabled)) {
                    if (enabled) {
                        DisabledFilters.RemoveAll(a => a == localizationKey);
                    } else {
                        DisabledFilters.Add(localizationKey);
                    }

                    Save();
                }
            }

            ImGui.EndChild();

            ImGui.End();
            return drawConfig;
        }
    }
}
