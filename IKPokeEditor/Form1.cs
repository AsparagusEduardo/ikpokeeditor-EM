﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using PoryPoke.DataLoad;

namespace PoryPoke
{
    public partial class Form1 : Form
    {

        string directory = null;
        string mayusPokemonName;
        string minusPokemonName;
        string firstMayusPokemonName;
        string fileName = null;
        string pathErr = null;
        string fileErr = null;

        bool isPokemonExpansion = false;

        //Dictionaries
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        Dictionary<string, string> data = new Dictionary<string, string>();
        Dictionary<string, Dictionary<string, string>> pokemonData = new Dictionary<string, Dictionary<string, string>>();
        Dictionary<string, Class.Pokemon> PokemonDictionary = new Dictionary<string, Class.Pokemon>();
        Dictionary<string, string[]> CustomBaseStats = new Dictionary<string, string[]>();
        Dictionary<string, Dictionary<string, string>> infoData = new Dictionary<string, Dictionary<string, string>>();
        Dictionary<string, Tuple<string, string, string, string>> evolutionData = new Dictionary<string, Tuple<string, string, string, string>>();
        Dictionary<string, Tuple<string, string, string>> moveData = new Dictionary<string, Tuple<string, string, string>>();
        Dictionary<string, Tuple<string, string>> mtmoData = new Dictionary<string, Tuple<string, string>>();

        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(1027, 619);
            Configuration.LoadConfig();
            menuSavingEmptyBaseStatsToolStripMenuItem.Checked = bool.Parse(Configuration.Config["hideEmptyBaseStats"]);
            cmbInforma_Species.Enabled = false;
            switchFormElementState(false);
            Configuration.LoadLanguageFiles();
            LoadLanguage(Configuration.Config["language"]);
        }

        private void seleccionarCarpetaToolStripMenuItem_Click(object sender, EventArgs e)
        {

            CommonOpenFileDialog FBD = new CommonOpenFileDialog();
            FBD.IsFolderPicker = true;
            FBD.RestoreDirectory = true;

            if (FBD.ShowDialog() == CommonFileDialogResult.Ok)
            {
                cleanAll();
                fileName = FBD.FileName;
                directory = FBD.FileName;
                bool checkResult = checkFilesExists();

                if (checkResult == true)
                {
                    //MessageBox.Show("Archivo encontrado");
                    setDataDictionary();
                    setPokemonDataDictionary();
                    setInfoDataDictionary();
                    loadData();
                    menuSaveToolStripMenuItem.Enabled = true;

                    cmbInforma_Species.SelectedIndex = 1;
                    cmbPokedex_Species.SelectedIndex = 1;
                    cmbGraphic_Species.SelectedIndex = 1;
                    cmbInforma_Species.Enabled = true;
                }
                else
                {
                    MessageBox.Show("No se ha encontrado el archivo " + fileErr + " en el directorio " + pathErr);
                }
            }
        }

        private bool checkFilesExists()
        {
            var rValue = true;
            var pathToSearch = "";

            Dictionary<string, string> filePaths = Constants.FilePaths;

            List<string> folderPathsList = new List<string>();
            /*string[] folderPaths =
            {
                (directory.ToString() + "\\graphics\\pokemon"),
                (directory.ToString() + "\\graphics\\pokemon_icon_palettes"),
            };*/
            /*
            folderPathsList.Add(directory.ToString() + "\\graphics\\pokemon");
            folderPathsList.Add(directory.ToString() + "\\graphics\\pokemon_icon_palettes");

            if (Directory.Exists((directory.ToString() + "\\sound\\cries")))
            {
                folderPathsList.Add((directory.ToString() + "\\sound\\cries"));
            } else if (Directory.Exists((directory.ToString() + "\\sound\\direct_sound_samples\\cries")))
            {
                folderPathsList.Add((directory.ToString() + "\\sound\\direct_sound_samples\\cries"));
            }
            */

            foreach (KeyValuePair<string, string> entry in filePaths)
            {
                string path = directory.ToString() + entry.Value;
                if (File.Exists(path) == false)
                {
                    rValue = false;
                    pathErr = path.Substring(0, path.LastIndexOfAny(new char[] { '\\', '/' }));
                    fileErr = path.Substring(path.LastIndexOfAny(new char[] { '\\', '/' }) + 1);
                }
                //pathToSearch = path;

                //pathToSearch = pathToSearch.Substring(pathToSearch.LastIndexOfAny(new char[] { '\\', '/' }) + 1);
                //pathToSearch = pathToSearch.Substring(0, pathToSearch.LastIndexOfAny(new char[] { '.' })) + "_" + pathToSearch.Substring(pathToSearch.LastIndexOfAny(new char[] { '.' }) + 1);

                dictionary[entry.Key] = path;
                data[entry.Key] = null;

                //MessageBox.Show("pFile_" + pathToSearch + " directory: " + dictionary["pFile_" + pathToSearch.ToString()].ToString());

            }

            for (int i = 0; i < folderPathsList.Count; i++)
            {
                if (Directory.Exists(folderPathsList[i]) == false)
                {
                    rValue = false;
                    pathErr = folderPathsList[i];
                }

                pathToSearch = folderPathsList[i];

                pathToSearch = pathToSearch.Substring(pathToSearch.LastIndexOfAny(new char[] { '\\', '/' }) + 1);

                dictionary["pFolder_" + pathToSearch.ToString()] = folderPathsList[i];

                //MessageBox.Show("pFolder_" + pathToSearch + " directory: " + dictionary["pFolder_" + pathToSearch.ToString()].ToString());

                //MessageBox.Show("Folder: " + pathToSearch);
            }

            return rValue;
        }

        private void loadData()
        {
            loadPokemonBaseStats();
            loadPokemonNames();
            //loadPokemonEvolutions();
            //loadPokemonMovements();
            //loadPokemonMTMO();
            //loadDexData();
            //loadSpriteData();
            ////addArgumentsData();
            //addMovementsData();
            //addMTMOData();
        }

        private void loadPokemonNames()
        {
            var str = data["pFile_species_h"];
            int pastValue = 0;
            int pokeAmount = 0;
            string pokeName = "";

            pokeAmount = Regex.Matches(str, "#define SPECIES_").Cast<Match>().Count() - 29;

            cmbInforma_Species.Items.Clear();
            cmbPokedex_Species.Items.Clear();
            cmbGraphic_Species.Items.Clear();
            Evolucion.Items.Clear();

            for (int i = 0; i <= pokeAmount; i++)
            {
                int index = str.IndexOf("#define SPECIES_", pastValue + 1);
                pastValue = index;
                pokeName = str.Substring(index + 16, str.IndexOf(" ", index + 9) - index - 16);
                if (!pokeName.Equals("EGG"))
                {
                    cmbInforma_Species.Items.Insert(i, pokeName);
                    cmbPokedex_Species.Items.Insert(i, pokeName);
                    cmbGraphic_Species.Items.Insert(i, pokeName);
                    Evolucion.Items.Insert(i, pokeName);
                }
            }
        }

        private void loadPokemonBaseStats()
        {
            var str = data["pFile_base_stats_h"];
            var speciesNames = data["pFile_species_names_h"];

            DataLoad.pokeemerald.LoadMonBaseStats(str, speciesNames, ref PokemonDictionary);

            for (int i = 0; i < 13; i++)
            {
                /*

                indexName = (speciesNames.IndexOf("SPECIES_", pastValueName + 2));
                pastValueName = indexName;
                pokemonName = speciesNames.Substring((speciesNames.IndexOf("_(", indexName)) + 3, (speciesNames.IndexOf(",", indexName)) - ((speciesNames.IndexOf("_(", indexName)) + 3) - 2);
                pokemonData["pokemonName"][(i + 1).ToString()] = pokemonName;
                //*/
            }
        }

        private void loadPokemonEvolutions()
        {
            var str = data["pFile_evolution_h"];
            var index = 0;
            var evoIndex = 0;
            var evoAmount = 0;
            var pokemonIndex = "0";
            string workString = null;
            var pokemonToEvolve = "";
            var evoMethod = "";
            var argument = "";
            var evolution = "";

            var evolveAmount = Regex.Matches(str, @"\[SPECIES").Cast<Match>().Count() - 1;
            index = str.IndexOf("[SPECIES_", 0);
            //MessageBox.Show(index.ToString());

            //MessageBox.Show(pokeAmount.ToString());

            for (int i = 0; i <= evolveAmount; i++)
            {
                if (i != evolveAmount)
                {
                    workString = str.Substring(index, (str.IndexOf("[SPECIES_", (index + 1)) - index));
                }
                else
                {
                    //MessageBox.Show(index.ToString());
                    workString = str.Substring(index, (str.IndexOf("};", index) - index));
                }
                index = str.IndexOf("[SPECIES_", (index + 2));

                evoAmount = Regex.Matches(workString, "EVO_").Cast<Match>().Count();

                for (int j = 0; j < evoAmount; j++)
                {
                    if (evoIndex == 0)
                    {
                        pokemonToEvolve = workString.Substring((workString.IndexOf("[SPECIES_", evoIndex) + 9), (workString.IndexOf("]", evoIndex) - (workString.IndexOf("[SPECIES_", evoIndex) + 9)));
                        evoIndex = (workString.IndexOf("[SPECIES_", evoIndex));
                    }
                    //MessageBox.Show(pokemonToEvolve);
                    evoMethod = workString.Substring(workString.IndexOf("EVO_", evoIndex), (workString.IndexOf(",", evoIndex) - workString.IndexOf("EVO_", evoIndex)));
                    evoIndex = workString.IndexOf("EVO_", evoIndex);
                    var firstComma = workString.IndexOf(",", evoIndex);
                    argument = workString.Substring((firstComma + 2), workString.IndexOf(",", firstComma + 1) - (firstComma + 2));
                    evoIndex = workString.IndexOf(",", firstComma + 1);
                    evolution = workString.Substring(evoIndex + 2, workString.IndexOf("}", evoIndex) - (evoIndex + 2));
                    evoIndex = workString.IndexOf("},", evoIndex) + 2;
                    if (j == (evoAmount - 1)) { evoIndex = 0; }

                    if (pokemonToEvolve == "NIDORAN_F")
                    {
                        pokemonIndex = "29";
                    }
                    else if (pokemonToEvolve == "NIDORAN_M")
                    {
                        pokemonIndex = "32";
                    }
                    else
                    {
                        pokemonIndex = (pokemonData["pokemonName"].FirstOrDefault(x => x.Value.Contains(pokemonToEvolve)).Key);
                    }

                    evolutionData[pokemonIndex + "_" + j.ToString()] = Tuple.Create(evoAmount.ToString(), evoMethod, argument, evolution);

                }

            }

        }

        private void loadPokemonMovements()
        {
            var str = data["pFile_level_up_learnsets_h"];
            var index = 0;
            var moveIndex = 0;
            var moveAmount = 0;
            string workString = null;

            var pokemonAmount = Regex.Matches(str, "LevelUpLearnset").Cast<Match>().Count();
            index = str.IndexOf("LevelUpLearnset", 0);

            for (int i = 0; i < pokemonAmount; i++)
            {
                workString = str.Substring(str.IndexOf("LEVEL_UP_MOVE", index), str.IndexOf("LEVEL_UP_END", index) - str.IndexOf("LEVEL_UP_MOVE", index));
                index = str.IndexOf("LEVEL_UP_MOVE", str.IndexOf("LEVEL_UP_END", index));
                moveAmount = Regex.Matches(workString, "LEVEL_UP_MOVE").Cast<Match>().Count();

                moveIndex = 0;

                for (int j = 0; j < moveAmount; j++)
                {
                    var movementLevel = workString.Substring((workString.IndexOf("LEVEL_UP_MOVE(", moveIndex) + 14), workString.IndexOf(",", moveIndex) - (workString.IndexOf("LEVEL_UP_MOVE(", moveIndex) + 14));
                    movementLevel = movementLevel.Replace(@" ", "");
                    moveIndex = workString.IndexOf(",", moveIndex) + 1;
                    var movementName = workString.Substring((workString.IndexOf("MOVE_", moveIndex)), (workString.IndexOf(",", moveIndex) - 1) - (workString.IndexOf("MOVE_", moveIndex)));
                    moveIndex = workString.IndexOf(",", moveIndex) + 1;
                    //MessageBox.Show(movementName + " at level " + movementLevel);
                    moveData[(i + 1).ToString() + "_" + j.ToString()] = Tuple.Create(moveAmount.ToString(), movementLevel, movementName);
                }
                //MessageBox.Show(moveAmount.ToString());
            }
        }

        private void loadPokemonMTMO()
        {
            var str = data["pFile_tmhm_learnsets_h"];
            var index = 0;
            var moveIndex = 0;
            var moveAmount = 0;
            string workString = null;

            var pokemonAmount = Regex.Matches(str, @"\[SPECIES_").Cast<Match>().Count() - 1;
            index = str.IndexOf("[SPECIES_", 0) + 1;
            index = str.IndexOf("[SPECIES_", index) - 1;

            for (int i = 0; i < pokemonAmount; i++)
            {
                workString = str.Substring((str.IndexOf("[SPECIES_", index)), ((str.IndexOf(",", index)) - (str.IndexOf("[SPECIES_", index))));
                index = str.IndexOf(",", index) + 1;
                moveAmount = Regex.Matches(workString, @"TMHM\(").Cast<Match>().Count();
                //richTextBox1.Text += workString + Environment.NewLine + Environment.NewLine;

                moveIndex = 0;
                for (int j = 0; j < moveAmount; j++)
                {
                    var movementName = workString.Substring(workString.IndexOf("TMHM(", moveIndex), workString.IndexOf(")", moveIndex) - workString.IndexOf("TMHM(", moveIndex));
                    movementName = movementName.Substring(movementName.IndexOf("(", 0) + 1);
                    moveIndex = workString.IndexOf(")", moveIndex) + 1;

                    mtmoData[(i + 1).ToString() + "_" + j.ToString()] = Tuple.Create(moveAmount.ToString(), movementName);
                    //MessageBox.Show(movementName);
                }

            }

        }

        private void loadDexData()
        {
            loadDescriptions();
            loadGeneralDexData();
        }

        private void loadDescriptions()
        {
            var str = data["pFile_pokedex_entries_en_h"];
            var index = 0;
            var stringIndex = 0;
            var pokemonSpecie = "";
            var pokemonIndex = 0;
            var pokemonAmount = (Regex.Matches(str, "\n\nstatic const").Cast<Match>().Count() + Regex.Matches(str, "#else\nstatic const").Cast<Match>().Count()) + 2;

            for (int i = 0; i < pokemonAmount; i++)
            {
                stringIndex = 0;

                if (str.IndexOf("\n\nstatic const", index + 1) > 0)
                {
                    if (str.IndexOf("\n\nstatic const", index + 10) > str.IndexOf("#else\nstatic const", index + 10))
                    {
                        index = str.IndexOf("#else\nstatic const", index) + 5;

                    }
                    else
                    {
                        index = str.IndexOf("\n\nstatic const", index) + 1;
                    }
                }
                else
                {
                    index = str.IndexOf("#else\nstatic const", index) + 5;
                }


                var workString = str.Substring(index, (str.IndexOf(");", str.IndexOf("static const", index + 10)) - index + 2));
                pokemonSpecie = workString.Substring((workString.IndexOf("DexDescription_", 0) + "DexDescription_".Length), workString.IndexOf("_1", 0) - (workString.IndexOf("DexDescription_", 0) + "DexDescription_".Length));
                pokemonSpecie = Regex.Replace(pokemonSpecie, @"(\p{Lu})", " $1").TrimStart();
                if (pokemonSpecie == "Mrmime") { pokemonSpecie = "Mr Mime"; }
                pokemonSpecie = pokemonSpecie.ToUpper();

                var descriptionOne = workString.Substring((workString.IndexOf("_1[] = _(", stringIndex) + 10), (workString.IndexOf(");", stringIndex)) - (workString.IndexOf("_1[] = _(", stringIndex) + 10));
                stringIndex = workString.IndexOf(");", stringIndex) + 2;
                var descrpitionTwo = workString.Substring((workString.IndexOf("_2[] = _(", stringIndex) + 10), workString.IndexOf(");", stringIndex) - (workString.IndexOf("_2[] = _(", stringIndex) + 10));

                descriptionOne = descriptionOne.Replace("\\n", "");
                descriptionOne = descriptionOne.Replace("\n", " ");
                descriptionOne = descriptionOne.Replace("\"", "");
                descriptionOne = descriptionOne.Replace("  ", "");
                descrpitionTwo = descrpitionTwo.Replace("\\n", "");
                descrpitionTwo = descrpitionTwo.Replace("\n", " ");
                descrpitionTwo = descrpitionTwo.Replace("\"", "");
                descrpitionTwo = descrpitionTwo.Replace("  ", "");

                if (cmbInforma_Species.Items.Contains(pokemonSpecie))
                {
                    pokemonIndex = cmbInforma_Species.Items.IndexOf(pokemonSpecie);
                }

                pokemonData["pokedexPageOne"][pokemonIndex.ToString()] = descriptionOne;
                pokemonData["pokedexPageTwo"][pokemonIndex.ToString()] = descrpitionTwo;
            }
        }

        private void loadGeneralDexData()
        {
            var str = data["pFile_pokedex_entries_en_h"];
            var index = 0;
            var stringIndex = 0;
            var pokemonIndex = 0;
            string pokemonName = "";
            string categoria = "";
            string altura = "";
            string peso = "";
            string escalaPokemon = "";
            string offsetPokemon = "";
            string escalaEntrenador = "";
            string offsetEntrenador = "";
            var strPal = data["pFile_pokemon_icon_c"];

            var pokemonAmount = Regex.Matches(str, ".categoryName").Cast<Match>().Count() - 1;
            var pokemonAmount2 = Regex.Matches(strPal, "gMonIcon").Cast<Match>().Count() - 1;

            index = str.IndexOf(".categoryName", 0) + 1;
            index = str.IndexOf("},", index) + 2;

            for (int i = 0; i < pokemonAmount; i++)
            {
                var workString = str.Substring(str.IndexOf("{", index), str.IndexOf("},", index) - str.IndexOf("{", index) + 2);
                index = str.IndexOf("},", index) + 2;
                stringIndex = 0;
                pokemonName = workString.Substring((workString.IndexOf("DexDescription_", 0) + 15), workString.IndexOf("_1", 0) - (workString.IndexOf("DexDescription_", 0) + 15));
                pokemonName = Regex.Replace(pokemonName, @"(\p{Lu})", " $1").TrimStart();
                if (pokemonName == "Mrmime") { pokemonName = "Mr Mime"; }
                pokemonName = pokemonName.ToUpper();

                if (cmbInforma_Species.Items.Contains(pokemonName))
                {
                    pokemonIndex = cmbInforma_Species.Items.IndexOf(pokemonName);
                }

                categoria = workString.Substring((workString.IndexOf("categoryName", stringIndex) + 18), workString.IndexOf("\"),", stringIndex) - (workString.IndexOf("categoryName", stringIndex) + 18));
                pokemonData["categoriaPokemon"][pokemonIndex.ToString()] = categoria;
                stringIndex = workString.IndexOf("\"),", stringIndex) + 4;

                altura = workString.Substring((workString.IndexOf("height", stringIndex) + 9), workString.IndexOf(",", stringIndex) - (workString.IndexOf("height", stringIndex) + 9));
                pokemonData["altura"][pokemonIndex.ToString()] = altura;
                stringIndex = workString.IndexOf(",", stringIndex) + 2;

                peso = workString.Substring((workString.IndexOf("weight", stringIndex) + 9), workString.IndexOf(",", stringIndex) - (workString.IndexOf("weight", stringIndex) + 9));
                pokemonData["peso"][pokemonIndex.ToString()] = peso;
                stringIndex = workString.IndexOf("pokemonScale", stringIndex);

                escalaPokemon = workString.Substring((workString.IndexOf("pokemonScale", stringIndex) + 15), workString.IndexOf(",", stringIndex) - (workString.IndexOf("pokemonScale", stringIndex) + 15));
                pokemonData["escalaPokemon"][pokemonIndex.ToString()] = escalaPokemon;
                stringIndex = workString.IndexOf(",", stringIndex) + 2;

                offsetPokemon = workString.Substring((workString.IndexOf("pokemonOffset", stringIndex) + 16), workString.IndexOf(",", stringIndex) - (workString.IndexOf("pokemonOffset", stringIndex) + 16));
                pokemonData["offsetPokemon"][pokemonIndex.ToString()] = offsetPokemon;
                stringIndex = workString.IndexOf(",", stringIndex) + 2;

                escalaEntrenador = workString.Substring((workString.IndexOf("trainerScale", stringIndex) + 15), workString.IndexOf(",", stringIndex) - (workString.IndexOf("trainerScale", stringIndex) + 15));
                pokemonData["escalaEntrenador"][pokemonIndex.ToString()] = escalaEntrenador;
                stringIndex = workString.IndexOf(",", stringIndex) + 2;

                offsetEntrenador = workString.Substring((workString.IndexOf("trainerOffset", stringIndex) + 16), workString.IndexOf(",", stringIndex) - (workString.IndexOf("trainerOffset", stringIndex) + 16));
                pokemonData["offsetEntrenador"][pokemonIndex.ToString()] = offsetEntrenador;
                stringIndex = workString.IndexOf(",", stringIndex) + 2;
            }
            index = strPal.IndexOf("const u8 gMonIconPaletteIndices[] =", 0);
            index = strPal.IndexOf("{", index);
            for (int i = 0; i < pokemonAmount2; i++)
            {
                index = strPal.IndexOf("\n", index) + 5;
                var workString = strPal.Substring(index, strPal.IndexOf(",", index) - index);
                pokemonData["palUsed"][i.ToString()] = workString;
            }
        }

        private void loadSpriteData()
        {
            var bCString = data["pFile_back_pic_coords_inc"];
            var fCString = data["pFile_front_pic_coords_inc"];
            var elevateString = data["pFile_battle_1_c"];
            var index = 0;

            var backBytes = Regex.Matches(bCString, ".byte").Cast<Match>().Count() - 1;
            var frontBytes = Regex.Matches(fCString, ".byte").Cast<Match>().Count() - 1;
            var elevateCount = Regex.Matches(elevateString, @"\[SPECIES_").Cast<Match>().Count();
            var palAmount = Directory.GetFiles(dictionary["pFolder_pokemon_icon_palettes"], "*.pal", SearchOption.AllDirectories).Length;

            for (int i = 0; i < backBytes; i++)
            {
                index = bCString.IndexOf(".byte", index + 1);
                var backY = (bCString.Substring((bCString.IndexOf(",", index) + 1), (bCString.IndexOf(",", bCString.IndexOf(",", index) + 1)) - (bCString.IndexOf(",", index) + 1))).Replace(" ", "");
                pokemonData["backCord"][i.ToString()] = backY.ToString();
            }
            index = 0;
            for (int i = 0; i < frontBytes; i++)
            {
                index = fCString.IndexOf(".byte", index + 1);
                var frontY = (fCString.Substring((fCString.IndexOf(",", index) + 1), (fCString.IndexOf(",", fCString.IndexOf(",", index) + 1)) - (fCString.IndexOf(",", index) + 1))).Replace(" ", "");
                pokemonData["frontCord"][i.ToString()] = frontY.ToString();
            }
            index = 0;
            for (int i = 0; i < elevateCount; i++)
            {
                index = elevateString.IndexOf("[SPECIES_", index) + 9;
                //var pokemonName = elevateString.Substring(index, elevateString.IndexOf("]", index) - index);
                var elevation = elevateString.Substring((elevateString.IndexOf("=", index) + 1), (elevateString.IndexOf(",", index) - (elevateString.IndexOf("=", index) + 1)));
                //MessageBox.Show("POKéMON: " + pokemonName + "\nElevation: " + elevation);
                pokemonData["elevate"][(i + 1).ToString()] = elevation.ToString();
            }
            for (int i = 0; i < palAmount; i++)
            {
                iconPalette.Items.Add(i.ToString());
            }

        }

        private void debuggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getSizeValue(fondo.Image);
        }

        private string getSizeValue(Image imagen)
        {
            if (imagen != null)
            {
                Bitmap b = new Bitmap(imagen);

                bool found = false;
                Point firstY = new Point(0, 0);
                Point lastY = new Point(0, 0);
                Point firstX = new Point(0, 0);
                Point lastX = new Point(0, 0);
                int realX;
                int realY;
                //GET FIRST COLOR AT Y
                for (int x = 0; x < imagen.Width; x++)
                {
                    for (int y = 0; y < imagen.Height; y++)
                    {
                        if (found == false)
                        {
                            if (b.GetPixel(x, y).A != 0)
                            {
                                firstY = new Point(x, y);
                                found = true;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                found = false;
                //GET LAST COLOR AT Y
                for (int x = imagen.Width - 1; x >= 0; x--)
                {
                    for (int y = 0; y < imagen.Height; y++)
                    {
                        if (found == false)
                        {
                            if (b.GetPixel(x, y).A != 0)
                            {
                                lastY = new Point(x + 1, y + 1);
                                found = true;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                found = false;
                //GET FIRST COLOR AT X
                for (int y = 0; y < imagen.Height; y++)
                {
                    for (int x = 0; x < imagen.Width; x++)
                    {
                        if (found == false)
                        {
                            if (b.GetPixel(x, y).A != 0)
                            {
                                firstX = new Point(x, y);
                                found = true;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                found = false;
                //GET LAST COLOR AT X
                for (int y = imagen.Height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < imagen.Width; x++)
                    {
                        if (found == false)
                        {
                            if (b.GetPixel(x, y).A != 0)
                            {
                                lastX = new Point(x + 1, y + 1);
                                found = true;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                realX = (lastY.X - firstY.X);
                realY = (lastX.Y - firstX.Y);
                if ((realX % 8) != 0)
                {
                    realX += (8 - realX % 8);
                }
                if ((realY % 8) != 0)
                {
                    realY += (8 - realY % 8);
                }
                var tileX = realX / 8;
                var tileY = realY / 8;
                var binX = Convert.ToString(Convert.ToInt64(tileX), 2);
                var binY = Convert.ToString(Convert.ToInt64(tileY), 2);

                if (binX.Length < 4)
                {
                    for (int i = 0; i < (4 - binX.Length); i++)
                    {
                        binX = "0" + binX;
                    }
                }
                if (binY.Length < 4)
                {
                    for (int i = 0; i < (4 - binY.Length); i++)
                    {
                        binY = "0" + binY;
                    }
                }

                var binString = binX + binY;
                binString = (Convert.ToInt32(binString, 2)).ToString();

                //MessageBox.Show((Convert.ToInt32(binString, 2)).ToString());

                //watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;
                //MessageBox.Show("Real size\n\nx: " + realX + "\ny: " + realY + "\n\nTiempo transcurrido: " + elapsedMd);

                b.Dispose();

                return binString;
                //MessageBox.Show(colour.ToString());
            }
            else
            {
                return "0";
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            pictureBox2.Parent = fondo;
            pictureBox3.Parent = fondo;
            pictureBox2.BackColor = Color.Transparent;
            pictureBox3.BackColor = Color.Transparent;
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SorryNotImplemented();
            //generarPokemon();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveData();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            saveData();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            saveData();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cmbInforma_Species.SelectedIndex == 0) && (cmbInforma_Species.Text == "NONE"))
            {
                MessageBox.Show("No es posible cargar los datos de \"NONE\"");
                cmbInforma_Species.SelectedIndex = 1;
            }
            else
            {
                cmbPokedex_Species.SelectedIndex = cmbInforma_Species.SelectedIndex;
                cmbGraphic_Species.SelectedIndex = cmbInforma_Species.SelectedIndex;
                refreshInterface();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SorryNotImplemented();
            /*
            if ((dataGridView2.Rows.Count - 1) > 0)
            {
                dataGridView2.Rows.RemoveAt(Int32.Parse(EvolucionAEliminar.Value.ToString()));
            }
            if ((Int32.Parse(EvolucionAEliminar.Value.ToString())) == (Int32.Parse(evolutionData[cmbInforma_Species.SelectedIndex.ToString() + "_0"].Item1) - 1))
            {
                if ((dataGridView2.Rows.Count - 1) > 0 && (EvolucionAEliminar.Value > 0))
                {
                    EvolucionAEliminar.Value--;
                }
            }
            if (dataGridView2.Rows.Count > 1)
            {
                EvolucionAEliminar.Maximum = (dataGridView2.Rows.Count - 2);
            }
            else
            {
                EvolucionAEliminar.Maximum = 0;
            }
            */
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SorryNotImplemented();
            /*
            if ((dataGridView1.Rows.Count - 1) > 1)
            {
                dataGridView1.Rows.RemoveAt(Int32.Parse(MovimientoAEliminar.Value.ToString()));
            }
            if ((Int32.Parse(MovimientoAEliminar.Value.ToString())) == (Int32.Parse(moveData[cmbInforma_Species.SelectedIndex.ToString() + "_0"].Item1) - 1))
            {
                MovimientoAEliminar.Value--;
            }
            MovimientoAEliminar.Maximum = (dataGridView1.Rows.Count - 2);
            */
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if ((dataGridView3.Rows.Count - 1) > 0)
            {
                dataGridView3.Rows.RemoveAt(Int32.Parse(MTMOAEliminar.Value.ToString()));
            }
            if ((Int32.Parse(MTMOAEliminar.Value.ToString())) == (Int32.Parse(moveData[cmbInforma_Species.SelectedIndex.ToString() + "_0"].Item1) - 1))
            {
                MTMOAEliminar.Value--;
            }
            if ((dataGridView3.Rows.Count - 1) > 0)
            {
                MTMOAEliminar.Maximum = (dataGridView3.Rows.Count - 2);
            }
        }

        private void frontY_ValueChanged(object sender, EventArgs e)
        {
            setSpritePosition();
        }

        private void backY_ValueChanged(object sender, EventArgs e)
        {
            setSpritePosition();
        }

        private void Levitation_ValueChanged(object sender, EventArgs e)
        {
            setSpritePosition();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cmbPokedex_Species.SelectedIndex == 0) && (cmbInforma_Species.Text == "NONE"))
            {
                MessageBox.Show("No es posible cargar los datos de \"NONE\"");
                cmbInforma_Species.SelectedIndex = 1;
            }
            else
            {
                cmbGraphic_Species.SelectedIndex = cmbPokedex_Species.SelectedIndex;
                cmbInforma_Species.SelectedIndex = cmbPokedex_Species.SelectedIndex;
            }

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cmbPokedex_Species.SelectedIndex == 0) && (cmbInforma_Species.Text == "NONE"))
            {
                MessageBox.Show("No es posible cargar los datos de \"NONE\"");
                cmbInforma_Species.SelectedIndex = 1;
            }
            else
            {
                cmbPokedex_Species.SelectedIndex = cmbGraphic_Species.SelectedIndex;
                cmbInforma_Species.SelectedIndex = cmbGraphic_Species.SelectedIndex;
            }
        }

        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView3.Rows.Count > 1)
            {
                MTMOAEliminar.Maximum = dataGridView3.Rows.Count - 2;
            }
            else
            {
                MTMOAEliminar.Maximum = 0;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Rows.Count > 1)
            {
                MovimientoAEliminar.Maximum = dataGridView1.Rows.Count - 2;
            }
            else
            {
                MovimientoAEliminar.Maximum = 0;
            }
        }

        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView2.Rows.Count > 1)
            {
                EvolucionAEliminar.Maximum = dataGridView2.Rows.Count - 2;
            }
            else
            {
                EvolucionAEliminar.Maximum = 0;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    this.Size = new Size(1027, 619);
                    break;
                case 1:
                    SorryNotImplemented();
                    tabControl1.SelectedIndex = 0;
                    //this.Size = new Size(411, 525);
                    break;
                case 2:
                    SorryNotImplemented();
                    tabControl1.SelectedIndex = 0;
                    //this.Size = new Size(512, 356);
                    break;

            }
        }

        private void generoCheck_CheckedChanged(object sender, EventArgs e)
        {
            txtStats_genderRatio.Enabled = chkStats_gender.Checked;
        }

        private void detectIfNumber(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back)))
                e.Handled = true;
        }

        private bool checkIfNumeric(string someString)
        {
            foreach (char c in someString)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        private void detectIfNumberAndDecimal(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back) || (e.KeyChar == '.')))
                e.Handled = true;
        }

        private void switchFormElementState(bool enabled)
        {
            txtStats_baseHP.Enabled = enabled;
            txtStats_baseATK.Enabled = enabled;
            txtStats_baseDEF.Enabled = enabled;
            txtStats_baseSPEED.Enabled = enabled;
            txtStats_baseSPATK.Enabled = enabled;
            txtStats_baseSPDEF.Enabled = enabled;
            txtStats_evHP.Enabled = enabled;
            txtStats_evATK.Enabled = enabled;
            txtStats_evDEF.Enabled = enabled;
            txtStats_evSPEED.Enabled = enabled;
            txtStats_evSPATK.Enabled = enabled;
            txtStats_evSPDEF.Enabled = enabled;
            txtStats_catchRate.Enabled = enabled;
            txtStats_expYield.Enabled = enabled;
            ddlStats_type1.Enabled = enabled;
            ddlStats_type2.Enabled = enabled;
            ddlStats_item1.Enabled = enabled;
            ddlStats_item2.Enabled = enabled;
            chkStats_gender.Enabled = enabled;
            txtStats_genderRatio.Enabled = enabled;
            txtStats_eggCycles.Enabled = enabled;
            txtStats_friendship.Enabled = enabled;
            ddlStats_growthRate.Enabled = enabled;
            ddlStats_eggGroup1.Enabled = enabled;
            ddlStats_eggGroup2.Enabled = enabled;
            ddlStats_ability1.Enabled = enabled;
            ddlStats_ability2.Enabled = enabled;
            ddlStats_safariFleeRate.Enabled = enabled;
            ddlStats_bodyColor.Enabled = enabled;
        }

        private void refreshInterface()
        {
            string speciesID = cmbInforma_Species.SelectedItem.ToString();
            Class.Pokemon poke = new Class.Pokemon();
            try
            {
                poke = PokemonDictionary[speciesID];
            }
            catch (KeyNotFoundException)
            {
                MessageBox.Show("No data found for " + speciesID + ". Default values will now be created.", "Warning");
                poke.ID = speciesID;
                poke.Name = speciesID;
                PokemonDictionary.Add(speciesID, poke);
            }
            switchFormElementState(!poke.IsOldUnown);

            txtStats_baseHP.Text = poke.BaseHP.ToString();
            txtStats_baseATK.Text = poke.BaseAttack.ToString();
            txtStats_baseDEF.Text = poke.BaseDefense.ToString();
            txtStats_baseSPEED.Text = poke.BaseSpeed.ToString();
            txtStats_baseSPATK.Text = poke.BaseSpAttack.ToString();
            txtStats_baseSPDEF.Text = poke.BaseSpDefense.ToString();
            txtStats_evHP.Text = poke.EvHP.ToString();
            txtStats_evATK.Text = poke.EvAttack.ToString();
            txtStats_evDEF.Text = poke.EvDefense.ToString();
            txtStats_evSPEED.Text = poke.EvSpeed.ToString();
            txtStats_evSPATK.Text = poke.EvSpAttack.ToString();
            txtStats_evSPDEF.Text = poke.EvSpDefense.ToString();
            txtStats_catchRate.Text = poke.CatchRate.ToString();
            txtStats_expYield.Text = poke.ExpYield.ToString();
            ddlStats_type1.SelectedIndex = int.Parse(infoData["tipos"].FirstOrDefault(x => x.Value.Contains(poke.Type1.Substring(5))).Key);
            ddlStats_type2.SelectedIndex = int.Parse(infoData["tipos"].FirstOrDefault(x => x.Value.Contains(poke.Type2.Substring(5))).Key);
            ddlStats_item1.SelectedIndex = int.Parse(infoData["objetos"].FirstOrDefault(x => x.Value.Contains(poke.Item1.Substring(5).Replace("_", " "))).Key);
            ddlStats_item2.SelectedIndex = int.Parse(infoData["objetos"].FirstOrDefault(x => x.Value.Contains(poke.Item2.Substring(5).Replace("_", " "))).Key);
            txtStats_genderRatio.Enabled = chkStats_gender.Checked = poke.HasGender;
            txtStats_genderRatio.Text = poke.GenderRatio.ToString(new CultureInfo("en-US"));
            txtStats_eggCycles.Text = poke.EggCycles.ToString();
            txtStats_friendship.Text = poke.Friendship.ToString();

            ddlStats_growthRate.SelectedIndex = int.Parse(infoData["crecimiento"].FirstOrDefault(x => x.Value.Equals(poke.GrowthRate.Substring(7).Replace("_", " "))).Key);
            ddlStats_eggGroup1.SelectedIndex = int.Parse(infoData["grupos_huevo"].FirstOrDefault(x => x.Value.Contains(poke.EggGroup1.Substring(10).Replace("_", " "))).Key);
            ddlStats_eggGroup2.SelectedIndex = int.Parse(infoData["grupos_huevo"].FirstOrDefault(x => x.Value.Contains(poke.EggGroup2.Substring(10).Replace("_", " "))).Key);
            ddlStats_ability1.SelectedIndex = int.Parse(infoData["habilidades"].FirstOrDefault(x => x.Value.Contains(poke.Ability1.Substring(8).Replace("_", " "))).Key);
            ddlStats_ability2.SelectedIndex = int.Parse(infoData["habilidades"].FirstOrDefault(x => x.Value.Contains(poke.Ability2.Substring(8).Replace("_", " "))).Key);
            if (isPokemonExpansion)
            {
                ddlStats_abilityHidden.SelectedIndex = int.Parse(infoData["habilidades"].FirstOrDefault(x => x.Value.Contains(poke.AbilityHidden.Substring(8).Replace("_", " "))).Key);
                ddlStats_abilityHidden.Enabled = true;
            }
            else
            {
                ddlStats_abilityHidden.Enabled = false;
                ddlStats_abilityHidden.Text = "";
            }
            ddlStats_safariFleeRate.Text = poke.SafariFleeRate.ToString();
            ddlStats_bodyColor.SelectedIndex = int.Parse(infoData["color_cuerpo"].FirstOrDefault(x => x.Value.Contains(poke.BodyColor.Substring(11))).Key);

            /*  

            POKEMON_NAME.Text = pokemonData["pokemonName"][cmbInforma_Species.SelectedIndex.ToString()];

            //EVOLUTION
            this.dataGridView2.Rows.Clear();
            var evosAmount = 0;
            if (evolutionData.ContainsKey(cmbInforma_Species.SelectedIndex.ToString() + "_0") == true)
            {
                evosAmount = Int32.Parse((evolutionData[cmbInforma_Species.SelectedIndex.ToString() + "_0"].Item1));
            }
            else
            {
                evosAmount = 0;
            }
            if (evosAmount > 0) {
                EvolucionAEliminar.Maximum = evosAmount - 1;
            } else
            {
                EvolucionAEliminar.Maximum = 0;
            }

            for (int i = 0; i < evosAmount; i++)
            {
                if (evolutionData.ContainsKey(cmbInforma_Species.SelectedIndex.ToString() + "_0") == true)
                {
                    var method = ((evolutionData[cmbInforma_Species.SelectedIndex.ToString() + "_" + i.ToString()].Item2).Replace(@"_", " ")).Substring(4);
                    var argument = (evolutionData[cmbInforma_Species.SelectedIndex.ToString() + "_" + i.ToString()].Item3).Replace(@" ", "");
                    var evolution = ((evolutionData[cmbInforma_Species.SelectedIndex.ToString() + "_" + i.ToString()].Item4).Replace(@"_", " ")).Substring(8);
                    this.dataGridView2.Rows.Add(method, argument, evolution);
                }
            }

            //MOVEMENTS

            this.dataGridView1.Rows.Clear();

            var moveAmount = moveData[cmbInforma_Species.SelectedIndex.ToString() + "_0"].Item1;
            for (int i = 0; i < Int32.Parse(moveAmount); i++)
            {
                var level = moveData[cmbInforma_Species.SelectedIndex.ToString() + "_" + i.ToString()].Item2;
                var movement = (moveData[cmbInforma_Species.SelectedIndex.ToString() + "_" + i.ToString()].Item3).Substring(5).Replace(@"_", " ");
                this.dataGridView1.Rows.Add(movement, level);
            }
            MovimientoAEliminar.Maximum = Int32.Parse(moveAmount) - 1;

            //MT/MO
            this.dataGridView3.Rows.Clear();
            var mtmoAmount = 0;

            if (mtmoData.ContainsKey(cmbInforma_Species.SelectedIndex.ToString() + "_0") == true) {
                mtmoAmount = Int32.Parse(mtmoData[cmbInforma_Species.SelectedIndex.ToString() + "_0"].Item1);
            } else
            {
                mtmoAmount = 0;
            }

            if (mtmoAmount > 0)
            {
                MTMOAEliminar.Maximum = mtmoAmount - 1;
            }
            else
            {
                MTMOAEliminar.Maximum = 0;
            }

            for (int i = 0; i < mtmoAmount; i++)
            {
                var mtmo = mtmoData[cmbInforma_Species.SelectedIndex.ToString() + "_" + i.ToString()].Item2.Replace(@"_", " ");
                this.dataGridView3.Rows.Add(mtmo);
            }



            //POKEDEX INFORMATION
            if (pokemonData["pokedexPageOne"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                descripcionUno.Text = pokemonData["pokedexPageOne"][cmbInforma_Species.SelectedIndex.ToString()];
            } else
            {
                descripcionUno.Text = "";
            }
            if (pokemonData["pokedexPageTwo"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                descripcionDos.Text = pokemonData["pokedexPageTwo"][cmbInforma_Species.SelectedIndex.ToString()];
            } else
            {
                descripcionDos.Text = "";
            }

            if (pokemonData["categoriaPokemon"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                categoriaPokemon.Text = pokemonData["categoriaPokemon"][cmbInforma_Species.SelectedIndex.ToString()];
            }
            else
            {
                categoriaPokemon.Text = "";
            }

            if (pokemonData["altura"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                altura.Text = pokemonData["altura"][cmbInforma_Species.SelectedIndex.ToString()];
            }
            else
            {
                altura.Text = "";
            }

            if (pokemonData["peso"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                peso.Text = pokemonData["peso"][cmbInforma_Species.SelectedIndex.ToString()];
            }
            else
            {
                peso.Text = "";
            }

            if (pokemonData["escalaPokemon"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                escalaPokemon.Text = pokemonData["escalaPokemon"][cmbInforma_Species.SelectedIndex.ToString()];
            }
            else
            {
                escalaPokemon.Text = "";
            }

            if (pokemonData["offsetPokemon"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                offsetPokemon.Text = pokemonData["offsetPokemon"][cmbInforma_Species.SelectedIndex.ToString()];
            }
            else
            {
                offsetPokemon.Text = "";
            }

            if (pokemonData["escalaEntrenador"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                escalaEntrenador.Text = pokemonData["escalaEntrenador"][cmbInforma_Species.SelectedIndex.ToString()];
            }
            else
            {
                escalaEntrenador.Text = "";
            }

            if (pokemonData["offsetEntrenador"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                offsetEntrenador.Text = pokemonData["offsetEntrenador"][cmbInforma_Species.SelectedIndex.ToString()];
            }
            else
            {
                offsetEntrenador.Text = "";
            }

            //SPRITES
            frontY.Value = Int32.Parse(pokemonData["frontCord"][cmbInforma_Species.SelectedIndex.ToString()]);
            backY.Value = Int32.Parse(pokemonData["backCord"][cmbInforma_Species.SelectedIndex.ToString()]);
            Levitation.Value = Int32.Parse(pokemonData["elevate"][cmbInforma_Species.SelectedIndex.ToString()]);
            iconPalette.SelectedIndex = Int32.Parse(pokemonData["palUsed"][cmbInforma_Species.SelectedIndex.ToString()]);
            var formatPokemonName = (cmbInforma_Species.Text).ToString().ToLower().Replace(" ", "_");

            bool backExist = File.Exists(dictionary["pFolder_pokemon"] + "\\" + formatPokemonName + "\\back.png");
            bool frontExist = File.Exists(dictionary["pFolder_pokemon"] + "\\" + formatPokemonName + "\\front.png");
            bool footprintExist = File.Exists(dictionary["pFolder_pokemon"] + "\\" + formatPokemonName + "\\footprint.png");
            bool iconExist = File.Exists(dictionary["pFolder_pokemon"] + "\\" + formatPokemonName + "\\icon.png");

            if (backExist) {
                pictureBox2.Image = Image.FromFile(dictionary["pFolder_pokemon"] + "\\" + formatPokemonName + "\\back.png");
            } else
            {
                pictureBox2.Image = null;
            }
            if (frontExist) {
                pictureBox3.Image = Image.FromFile(dictionary["pFolder_pokemon"] + "\\" + formatPokemonName + "\\front.png");
            }
            else
            {
                pictureBox3.Image = null;
            }
            if (footprintExist)
            {
                footprint.Image = Image.FromFile(dictionary["pFolder_pokemon"] + "\\" + formatPokemonName + "\\footprint.png");
            }
            else
            {
                footprint.Image = null;
            }
            if (iconExist)
            {
                icon.Image = Image.FromFile(dictionary["pFolder_pokemon"] + "\\" + formatPokemonName + "\\icon.png");
            }
            else
            {
                icon.Image = null;
            }
            if (backExist && frontExist)
            {
                setSpritePosition();
            }
            */


            /*pokemonData.Add("categoriaPokemon", new Dictionary<string, string>());
            pokemonData.Add("altura", new Dictionary<string, string>());
            pokemonData.Add("peso", new Dictionary<string, string>());
            pokemonData.Add("escalaPokemon", new Dictionary<string, string>());
            pokemonData.Add("offsetPokemon", new Dictionary<string, string>());
            pokemonData.Add("escalaEntrenador", new Dictionary<string, string>());
            pokemonData.Add("offsetEntrenador", new Dictionary<string, string>());*/


        }

        //SET FUNCTIONS

        private void setBackPicTable()
        {
            //obj_tiles gMonBackPic_Chimecho, 0x800, SPECIES_CHIMECHO

            string str = null;

            StreamReader sr = new StreamReader(dictionary["pFile_back_pic_table_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var index = str.IndexOf("obj_tiles gMonPic_Egg, 0x800, SPECIES_EGG");
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            str = preStr + "obj_tiles gMonBackPic_" + System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(POKEMON_NAME.Text.ToLower()) + ", 0x800, SPECIES_" + POKEMON_NAME.Text.ToUpper() + Environment.NewLine + "\t" + postStr;
            data["pFile_back_pic_table_inc"] = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_back_pic_table_inc"].ToString(), false);
            sw.WriteLine(data["pFile_back_pic_table_inc"]);
            sw.Close();
        }

        private void setPokemonDataDictionary()
        {
            string str = data["pFile_pokemon_structs"].ToString();
            DataLoad.pokeemerald.LoadPokemonStruct(str, ref CustomBaseStats, ref isPokemonExpansion);

            /*
            pokemonData.Add("pokemonName", new Dictionary<string, string>());
            pokemonData.Add("psBase", new Dictionary<string, string>());
            pokemonData.Add("ataqueBase", new Dictionary<string, string>());
            pokemonData.Add("defensaBase", new Dictionary<string, string>());
            pokemonData.Add("velocidadBase", new Dictionary<string, string>());
            pokemonData.Add("ataqueEspecialBase", new Dictionary<string, string>());
            pokemonData.Add("defensaEspecialBase", new Dictionary<string, string>());
            pokemonData.Add("tipoUno", new Dictionary<string, string>());
            pokemonData.Add("tipoDos", new Dictionary<string, string>());
            pokemonData.Add("ratioDeCaptura", new Dictionary<string, string>());
            pokemonData.Add("expBase", new Dictionary<string, string>());
            pokemonData.Add("evsPS", new Dictionary<string, string>());
            pokemonData.Add("evsAtaque", new Dictionary<string, string>());
            pokemonData.Add("evsDefensa", new Dictionary<string, string>());
            pokemonData.Add("evsVelocidad", new Dictionary<string, string>());
            pokemonData.Add("evsAtaqueEspecial", new Dictionary<string, string>());
            pokemonData.Add("evsDefensaEspecial", new Dictionary<string, string>());
            pokemonData.Add("objetoUno", new Dictionary<string, string>());
            pokemonData.Add("objetoDos", new Dictionary<string, string>());
            pokemonData.Add("ratioGenero", new Dictionary<string, string>());
            pokemonData.Add("tieneGenero", new Dictionary<string, string>());
            pokemonData.Add("ciclosHuevo", new Dictionary<string, string>());
            pokemonData.Add("amistadBase", new Dictionary<string, string>());
            pokemonData.Add("crecimiento", new Dictionary<string, string>());
            pokemonData.Add("grupoHuevoUno", new Dictionary<string, string>());
            pokemonData.Add("grupoHuevoDos", new Dictionary<string, string>());
            pokemonData.Add("habilidadUno", new Dictionary<string, string>());
            pokemonData.Add("habilidadDos", new Dictionary<string, string>());
            pokemonData.Add("probabilidadHuidaSafari", new Dictionary<string, string>());
            pokemonData.Add("colorCuerpo", new Dictionary<string, string>());
            pokemonData.Add("pokedexPageOne", new Dictionary<string, string>());
            pokemonData.Add("pokedexPageTwo", new Dictionary<string, string>());
            pokemonData.Add("categoriaPokemon", new Dictionary<string, string>());
            pokemonData.Add("altura", new Dictionary<string, string>());
            pokemonData.Add("peso", new Dictionary<string, string>());
            pokemonData.Add("escalaPokemon", new Dictionary<string, string>());
            pokemonData.Add("offsetPokemon", new Dictionary<string, string>());
            pokemonData.Add("escalaEntrenador", new Dictionary<string, string>());
            pokemonData.Add("offsetEntrenador", new Dictionary<string, string>());
            pokemonData.Add("backCord", new Dictionary<string, string>());
            pokemonData.Add("frontCord", new Dictionary<string, string>());
            pokemonData.Add("elevate", new Dictionary<string, string>());
            pokemonData.Add("palUsed", new Dictionary<string, string>());
            */
        }

        private void cleanAll()
        {
            dictionary = new Dictionary<string, string>();
            data = new Dictionary<string, string>();
            pokemonData = new Dictionary<string, Dictionary<string, string>>();
            infoData = new Dictionary<string, Dictionary<string, string>>();
            evolutionData = new Dictionary<string, Tuple<string, string, string, string>>();
            moveData = new Dictionary<string, Tuple<string, string, string>>();
            mtmoData = new Dictionary<string, Tuple<string, string>>();

            directory = null;
            mayusPokemonName = null;
            minusPokemonName = null;
            firstMayusPokemonName = null;
            fileName = null;
            pathErr = null;
            fileErr = null;

            cmbInforma_Species.Items.Clear();
            cmbPokedex_Species.Items.Clear();
            cmbGraphic_Species.Items.Clear();
            ddlStats_type1.Items.Clear();
            ddlStats_type2.Items.Clear();
            ddlStats_eggGroup1.Items.Clear();
            ddlStats_eggGroup2.Items.Clear();
            ddlStats_ability1.Items.Clear();
            ddlStats_ability2.Items.Clear();
            ddlStats_abilityHidden.Items.Clear();
            ddlStats_item1.Items.Clear();
            ddlStats_item2.Items.Clear();
            iconPalette.Items.Clear();

            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();

        }

        private void setDataDictionary()
        {
            string str = null;
            Dictionary<string, string> filePaths = Constants.FilePaths;
            foreach (KeyValuePair<string, string> entry in filePaths)
            {
                StreamReader sr = new StreamReader(dictionary[entry.Key].ToString());
                str = sr.ReadToEnd();
                data[entry.Key] = str;
                sr.Close();
            }

            //richTextBox1.Text = data["pFile_base_stats_h"];
        }

        private void setInfoDataDictionary()
        {
            infoData.Add("tipos", new Dictionary<string, string>());
            infoData.Add("movimientos", new Dictionary<string, string>());
            infoData.Add("mt", new Dictionary<string, string>());
            infoData.Add("mo", new Dictionary<string, string>());
            infoData.Add("objetos", new Dictionary<string, string>());
            infoData.Add("habilidades", new Dictionary<string, string>());
            infoData.Add("grupos_huevo", new Dictionary<string, string>());
            infoData.Add("color_cuerpo", new Dictionary<string, string>());
            infoData.Add("crecimiento", new Dictionary<string, string>());
            infoData.Add("metodoEvolutivo", new Dictionary<string, string>());

            setTypesData();
            /*setMovementsData();*/
            setItemsData();
            /*setMTMOData();*/
            setEggColorGrowthData();
            setAbilitiesData();
            /*setEvolutiveMethodData();*/
        }

        private void setTypesData()
        {
            string str = data["pFile_pokemon_h"].ToString();
            DataLoad.pokeemerald.LoadBaseStatConstant(str, "TYPE_", "tipos", ref infoData);
            addTypesToComboBox();
        }

        private void setMovementsData()
        {
            string str = data["pFile_move_names_en_h"].ToString();
            int index = 0;
            int lastIndex = 0;

            var totalMoves = Regex.Matches(str, "MOVE_").Cast<Match>().Count() - 1;

            for (int i = 0; i <= totalMoves; i++)
            {
                index = str.IndexOf("MOVE_", lastIndex + 2);
                lastIndex = index;

                var typeName = str.Substring((str.IndexOf("_", index) + 1), (str.IndexOf("=", index) - 2) - (str.IndexOf("_", index) + 1));

                infoData["movimientos"][i.ToString()] = typeName;
                //MessageBox.Show("Ataque: " + infoData["movimientos"][i.ToString()]);
            }
        }

        private void setItemsData()
        {
            string str = data["pFile_items_h"].ToString();
            DataLoad.pokeemerald.LoadItems(str, ref infoData);
            addItemsToComboBox();
        }

        private void setMTMOData()
        {
            string str = data["pFile_items_en_h"].ToString();
            int index = 0;
            int lastIndex = 0;

            var countMT = Regex.Matches(str, "ITEM_TM").Cast<Match>().Count() - 1;
            var countMO = Regex.Matches(str, "ITEM_HM").Cast<Match>().Count() - 1;

            for (int i = 0; i <= countMT; i++)
            {
                index = str.IndexOf(".itemId = ITEM_TM", lastIndex + 2);
                lastIndex = index;

                var mt = str.Substring((index + 15), ((str.IndexOf(",", index)) - (index + 15)));

                mt = mt.Replace(@"_", " ");

                infoData["mt"][i.ToString()] = mt;
                //MessageBox.Show("Item: " + infoData["mt"][i.ToString()]);
            }

            index = 0;
            lastIndex = 0;

            for (int i = 0; i <= countMO; i++)
            {
                index = str.IndexOf(".itemId = ITEM_HM", lastIndex + 2);
                lastIndex = index;

                var mo = str.Substring((index + 15), ((str.IndexOf(",", index)) - (index + 15)));

                mo = mo.Replace(@"_", " ");

                infoData["mo"][i.ToString()] = mo;
                //MessageBox.Show("Item: " + infoData["mo"][i.ToString()]);
            }
        }

        private void setEggColorGrowthData()
        {
            string str = data["pFile_pokemon_h"].ToString();
            DataLoad.pokeemerald.LoadBaseStatConstant(str, "EGG_GROUP_", "grupos_huevo", ref infoData);
            DataLoad.pokeemerald.LoadBaseStatConstant(str, "BODY_COLOR_", "color_cuerpo", ref infoData);
            DataLoad.pokeemerald.LoadBaseStatConstant(str, "GROWTH_", "crecimiento", ref infoData);
            addEggGroupToComboBox();
            addBodyColorToComboBox();
            addGrowthToComboBox();
        }

        private void setAbilitiesData()
        {
            string str = data["pFile_abilities_h"].ToString();
            DataLoad.pokeemerald.LoadBaseStatConstant(str, "ABILITY_", "habilidades", ref infoData);
            addAbilitiesToComboBox();
        }

        private void setEvolutiveMethodData()
        {
            string str = data["pFile_pokemon_h"].ToString();
            int index = 0;
            int lastIndex = 0;

            var totalEvolutiveMethods = Regex.Matches(str, "EVO_").Cast<Match>().Count() - 1;

            for (int i = 0; i <= totalEvolutiveMethods; i++)
            {
                index = str.IndexOf("EVO_", lastIndex + 2);
                lastIndex = index;

                var evoMethodName = str.Substring((index + 4), (str.IndexOf(" ", index) - (index + 4)));

                evoMethodName = evoMethodName.Replace(@"_", " ");

                infoData["metodoEvolutivo"][i.ToString()] = evoMethodName;
                //MessageBox.Show("Método evolutivo: " + infoData["metodoEvolutivo"][i.ToString()]);
            }

            addEvoToComboBox();
        }

        //ADD FUNCTIONS

        private void addTypesToComboBox()
        {
            int typesAmount = infoData["tipos"].Count;

            ddlStats_type1.Items.Clear();
            ddlStats_type2.Items.Clear();

            for (int i = 0; i < typesAmount; i++)
            {
                string insertTypeName = infoData["tipos"][i.ToString()];
                //MessageBox.Show(insertTypeName);
                ddlStats_type1.Items.Insert(i, insertTypeName);
                ddlStats_type2.Items.Insert(i, insertTypeName);
            }
        }

        private void addItemsToComboBox()
        {
            int objectAmount = infoData["objetos"].Count;

            ddlStats_item1.Items.Clear();
            ddlStats_item2.Items.Clear();

            for (int i = 0; i < objectAmount; i++)
            {
                string insertObjectName = infoData["objetos"][i.ToString()];
                //MessageBox.Show(insertObjectName);
                ddlStats_item1.Items.Insert(i, insertObjectName);
                ddlStats_item2.Items.Insert(i, insertObjectName);
            }
        }

        private void addGrowthToComboBox()
        {
            int growthAmount = infoData["crecimiento"].Count;

            ddlStats_growthRate.Items.Clear();

            for (int i = 0; i < growthAmount; i++)
            {
                string insertGrowthName = infoData["crecimiento"][i.ToString()];
                //MessageBox.Show(insertGrowthName);
                ddlStats_growthRate.Items.Insert(i, insertGrowthName);
            }
        }

        private void addEggGroupToComboBox()
        {
            int eggGroupAmount = infoData["grupos_huevo"].Count;

            ddlStats_eggGroup1.Items.Clear();
            ddlStats_eggGroup2.Items.Clear();

            for (int i = 0; i < eggGroupAmount; i++)
            {
                string insertEggGroupName = infoData["grupos_huevo"][i.ToString()];
                //MessageBox.Show(insertEggGroupName);
                ddlStats_eggGroup1.Items.Insert(i, insertEggGroupName);
                ddlStats_eggGroup2.Items.Insert(i, insertEggGroupName);
            }
        }

        private void addBodyColorToComboBox()
        {
            int bodyColorAmount = infoData["color_cuerpo"].Count;

            ddlStats_bodyColor.Items.Clear();

            for (int i = 0; i < bodyColorAmount; i++)
            {
                string insertBodyColorName = infoData["color_cuerpo"][i.ToString()];
                //MessageBox.Show(insertBodyColorName);
                ddlStats_bodyColor.Items.Insert(i, insertBodyColorName);
            }
        }

        private void addAbilitiesToComboBox()
        {
            int abilitiesAmount = infoData["habilidades"].Count;

            ddlStats_ability1.Items.Clear();
            ddlStats_ability2.Items.Clear();
            ddlStats_abilityHidden.Items.Clear();

            for (int i = 0; i < abilitiesAmount; i++)
            {
                string insertAbilityName = infoData["habilidades"][i.ToString()];
                //MessageBox.Show(insertAbilityName);
                ddlStats_ability1.Items.Insert(i, insertAbilityName);
                ddlStats_ability2.Items.Insert(i, insertAbilityName);
                ddlStats_abilityHidden.Items.Insert(i, insertAbilityName);
            }
        }

        private void addEvoToComboBox()
        {
            int evoMethodsAmount = infoData["metodoEvolutivo"].Count;

            Metodo.Items.Clear();

            for (int i = 0; i < evoMethodsAmount; i++)
            {
                string insertEvoMethodName = infoData["metodoEvolutivo"][i.ToString()];
                //MessageBox.Show(insertEvoMethodName);
                Metodo.Items.Insert(i, insertEvoMethodName);
            }
        }

        /*
        private void addArgumentsData()
        {
            int objectAmount = infoData["objetos"].Count;

            Argumento.Items.Clear();

            for (int i = 0; i < 254 + objectAmount; i++)
            {
                if (i <= 255)
                {
                    Argumento.Items.Insert(i, i.ToString());
                } else
                {
                    string insertObjectName = infoData["objetos"][(i - 256).ToString()];
                    Argumento.Items.Insert(i, insertObjectName);
                }

            }
        }*/

        private void addMovementsData()
        {
            int movementAmount = infoData["movimientos"].Count;

            Ataque.Items.Clear();
            Nivel.Items.Clear();

            for (int i = 0; i <= 100; i++)
            {
                Nivel.Items.Insert(i, i.ToString());
            }

            for (int i = 0; i < movementAmount; i++)
            {
                var movement = (infoData["movimientos"][i.ToString()]).Replace(@"_", " ");
                Ataque.Items.Insert(i, movement);
            }
        }

        private void addMTMOData()
        {
            int mtAmount = infoData["mt"].Count;
            int moAmount = infoData["mo"].Count;

            MTName.Items.Clear();

            for (int i = 0; i < (mtAmount + moAmount); i++)
            {
                if (i < mtAmount)
                {
                    var movement = (infoData["mt"][i.ToString()]);
                    MTName.Items.Insert(i, movement);
                }
                else
                {
                    var movement = (infoData["mo"][(i - mtAmount).ToString()]);
                    MTName.Items.Insert(i, movement);
                }
            }
        }

        //SAVE FUNCTIONS

        private void saveData()
        {
            setBaseStats();
            /*
            setEvolutions();
            setMovements();
            setMTMO();
            setDexDescription();
            setDexData();
            setSpriteData();
            reloadAllSavedData();
            */
        }
        private void saveAllData()
        {
            setAllBaseStats();
            /*
            setEvolutions();
            setMovements();
            setMTMO();
            setDexDescription();
            setDexData();
            setSpriteData();
            reloadAllSavedData();
            */
        }

        private void saveMonToDictionary()
        {
            Class.Pokemon poke = PokemonDictionary[cmbInforma_Species.Text];

            poke.BaseHP = int.Parse(txtStats_baseHP.Text);
            poke.BaseAttack = int.Parse(txtStats_baseATK.Text);
            poke.BaseDefense = int.Parse(txtStats_baseDEF.Text);
            poke.BaseSpeed = int.Parse(txtStats_baseSPEED.Text);
            poke.BaseSpAttack = int.Parse(txtStats_baseSPATK.Text);
            poke.BaseSpDefense = int.Parse(txtStats_baseSPDEF.Text);
            poke.Type1 = "TYPE_" + ddlStats_type1.Text.Replace(" ", "_");
            poke.Type2 = "TYPE_" + ddlStats_type2.Text.Replace(" ", "_");
            poke.CatchRate = int.Parse(txtStats_catchRate.Text);
            poke.ExpYield = int.Parse(txtStats_expYield.Text);
            poke.EvHP = int.Parse(txtStats_evHP.Text);
            poke.EvAttack = int.Parse(txtStats_evATK.Text);
            poke.EvDefense = int.Parse(txtStats_evDEF.Text);
            poke.EvSpeed = int.Parse(txtStats_evSPEED.Text);
            poke.EvSpAttack = int.Parse(txtStats_evSPATK.Text);
            poke.EvSpDefense = int.Parse(txtStats_evSPDEF.Text);
            poke.Item1 = "ITEM_" + ddlStats_item1.Text.Replace(" ", "_");
            poke.Item2 = "ITEM_" + ddlStats_item2.Text.Replace(" ", "_");

            if (chkStats_gender.Checked)
            {
                poke.HasGender = true;
                poke.GenderRatio = decimal.Parse(txtStats_genderRatio.Text, new CultureInfo("en-US"));
            }
            else
            {
                poke.HasGender = false;
                poke.GenderRatio = 0;
            }
            poke.HasGender = chkStats_gender.Checked;
            poke.GenderRatio = decimal.Parse(txtStats_genderRatio.Text, new CultureInfo("en-US"));

            poke.EggCycles = int.Parse(txtStats_eggCycles.Text);
            poke.Friendship = int.Parse(txtStats_friendship.Text);
            poke.GrowthRate = "GROWTH_" + ddlStats_growthRate.Text.Replace(" ", "_");
            poke.EggGroup1 = "EGG_GROUP_" + ddlStats_eggGroup1.Text.Replace(" ", "_");
            poke.EggGroup2 = "EGG_GROUP_" + ddlStats_eggGroup2.Text.Replace(" ", "_");
            poke.Ability1 = "ABILITY_" + ddlStats_ability1.Text.Replace(" ", "_");
            poke.Ability2 = "ABILITY_" + ddlStats_ability2.Text.Replace(" ", "_");
            if (isPokemonExpansion)
                poke.AbilityHidden = "ABILITY_" + ddlStats_abilityHidden.Text.Replace(" ", "_");
            else
                poke.AbilityHidden = null;
            poke.SafariFleeRate = int.Parse(ddlStats_safariFleeRate.Text);
            poke.BodyColor = "BODY_COLOR_" + ddlStats_bodyColor.Text.Replace(" ", "_");
        }

        private void setBaseStats()
        {
            Class.Pokemon poke = PokemonDictionary[cmbInforma_Species.Text];
            if (!poke.IsOldUnown)
            {
                saveMonToDictionary();

                StreamReader sr = new StreamReader(dictionary["pFile_base_stats_h"].ToString());
                string str = sr.ReadToEnd();
                sr.Close();

                string finalString = formatBaseStatString(poke, menuSavingEmptyBaseStatsToolStripMenuItem.Checked);

                var index = str.IndexOf("[SPECIES_" + poke.ID) - 4;

                if (index > 0)
                {
                    var index2 = str.IndexOf("    }", index) + 5;
                    var preStr = str.Substring(0, index);
                    var postStr = str.Substring(index2 + 1);
                    str = preStr + finalString + postStr.Substring(0, postStr.Length);
                    index = str.LastIndexOf("[SPECIES");
                    index = str.IndexOf("    },", index);
                }
                else
                {
                    index = str.IndexOf("};");
                    var preStr = str.Substring(0, index);
                    str = preStr + "\n" + finalString + "\n};\n";
                }

                StreamWriter sw = new StreamWriter(dictionary["pFile_base_stats_h"].ToString(), false);
                sw.Write(str);
                sw.Close();
            }
        }

        private string formatBaseStatString(Class.Pokemon poke, bool hideZeroes)
        {
            string genderRatio;
            if (!poke.HasGender)
                genderRatio = "MON_GENDERLESS";
            else if (poke.GenderRatio == 0)
                genderRatio = "MON_MALE";
            else if (poke.GenderRatio == 100)
                genderRatio = "MON_FEMALE";
            else
                genderRatio = "PERCENT_FEMALE(" + poke.GenderRatio.ToString(new CultureInfo("en-US")) + ")";

            string finalString;
            finalString = "    [SPECIES_" + poke.ID + "] =\n    ";
            finalString += "{\n        .baseHP        = " + poke.BaseHP;
            finalString += ",\n        .baseAttack    = " + poke.BaseAttack;
            finalString += ",\n        .baseDefense   = " + poke.BaseDefense;
            finalString += ",\n        .baseSpeed     = " + poke.BaseSpeed;
            finalString += ",\n        .baseSpAttack  = " + poke.BaseSpAttack;
            finalString += ",\n        .baseSpDefense = " + poke.BaseSpDefense;
            finalString += ",\n        .type1 = " + poke.Type1;
            finalString += ",\n        .type2 = " + poke.Type2;
            finalString += ",\n        .catchRate = " + poke.CatchRate;
            finalString += ",\n        .expYield = " + poke.ExpYield;
            if (!hideZeroes || poke.EvHP != 0)
                finalString += ",\n        .evYield_HP        = " + poke.EvHP;
            if (!hideZeroes || poke.EvAttack != 0)
                finalString += ",\n        .evYield_Attack    = " + poke.EvAttack;
            if (!hideZeroes || poke.EvDefense != 0)
                finalString += ",\n        .evYield_Defense   = " + poke.EvDefense;
            if (!hideZeroes || poke.EvSpeed != 0)
                finalString += ",\n        .evYield_Speed     = " + poke.EvSpeed;
            if (!hideZeroes || poke.EvSpAttack != 0)
                finalString += ",\n        .evYield_SpAttack  = " + poke.EvSpAttack;
            if (!hideZeroes || poke.EvSpDefense != 0)
                finalString += ",\n        .evYield_SpDefense = " + poke.EvSpDefense;
            if (!hideZeroes || !poke.Item1.Equals("ITEM_NONE"))
                finalString += ",\n        .item1 = " + poke.Item1;
            if (!hideZeroes || !poke.Item2.Equals("ITEM_NONE"))
                finalString += ",\n        .item2 = " + poke.Item2;
            finalString += ",\n        .genderRatio = " + genderRatio;
            finalString += ",\n        .eggCycles = " + poke.EggCycles;
            finalString += ",\n        .friendship = " + poke.Friendship;
            finalString += ",\n        .growthRate = " + poke.GrowthRate;
            finalString += ",\n        .eggGroup1 = " + poke.EggGroup1;
            finalString += ",\n        .eggGroup2 = " + poke.EggGroup2;
            finalString += ",\n        .abilities = {" + poke.Ability1 + ", " + poke.Ability2 + "}";
            if (isPokemonExpansion && (!hideZeroes || !poke.AbilityHidden.Equals("ABILITY_NONE")))
                finalString += ",\n        .abilityHidden = " + poke.AbilityHidden;
            if (!hideZeroes || poke.SafariFleeRate != 0)
                finalString += ",\n        .safariZoneFleeRate = " + poke.SafariFleeRate;
            finalString += ",\n        .bodyColor = " + poke.BodyColor;
            finalString += ",\n        .noFlip = " + poke.NoFlip.ToString().ToUpper() + ",\n    },";
            return finalString;
        }

        private void setAllBaseStats()
        {
            saveMonToDictionary();

            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_base_stats_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            StreamWriter sw = new StreamWriter(dictionary["pFile_base_stats_h"].ToString(), false);

            string prestr = str.Substring(0, str.IndexOf("[SPECIES_NONE] = {0},") + 21);
            sw.WriteLine(prestr);

            foreach (KeyValuePair<string, Class.Pokemon> entry in PokemonDictionary)
            {
                Class.Pokemon poke = entry.Value;
                string genderRatio;
                if (!poke.HasGender)
                    genderRatio = "MON_GENDERLESS";
                else if (poke.GenderRatio == 0)
                    genderRatio = "MON_MALE";
                else if (poke.GenderRatio == 100)
                    genderRatio = "MON_FEMALE";
                else
                    genderRatio = "PERCENT_FEMALE(" + poke.GenderRatio.ToString(new CultureInfo("en-US")) + ")";

                string finalString;

                if (poke.IsOldUnown)
                    finalString = "\n    [SPECIES_" + poke.ID + "] = OLD_UNOWN_BASE_STATS,";
                else
                    finalString = "\n" + formatBaseStatString(poke, menuSavingEmptyBaseStatsToolStripMenuItem.Checked);
                sw.WriteLine(finalString);
            }
            sw.Write("};\n");
            sw.Close();
        }

        private void setEvolutions()
        {
            //var evolutions = dataGridView2.Rows[0].Cells[2].Value.ToString();
            var totalEvolutions = dataGridView2.Rows.Count - 1;
            var pokemonSpecie = cmbInforma_Species.Text;

            string[] metodo = new string[totalEvolutions];
            string[] argumento = new string[totalEvolutions];
            string[] evolucion = new string[totalEvolutions];
            string finalString = null;

            if (totalEvolutions > 0)
            {

                var spaceValue = 11 - pokemonSpecie.Length;

                finalString = "    [SPECIES_" + pokemonSpecie.Replace(" ", "_") + "]";

                for (int i = 0; i < totalEvolutions; i++)
                {
                    metodo[i] = dataGridView2.Rows[i].Cells[0].Value.ToString().Replace(" ", "_");
                    argumento[i] = dataGridView2.Rows[i].Cells[1].Value.ToString();
                    evolucion[i] = dataGridView2.Rows[i].Cells[2].Value.ToString().Replace(" ", "_");
                }

                for (int i = 0; i < spaceValue; i++)
                {
                    finalString += " ";
                }

                finalString += "= {";

                for (int i = 0; i < totalEvolutions; i++)
                {
                    finalString += "{EVO_" + metodo[i] + ", " + argumento[i] + ", SPECIES_" + evolucion[i] + "}";
                    if (i == (totalEvolutions - 1))
                    {
                        finalString += "},";
                    }
                    else
                    {
                        finalString += ",\n                            ";
                    }
                }

                //richTextBox1.Text = finalString;

            }
            else
            {
                finalString = "";
            }


            string str = null;

            StreamReader sr = new StreamReader(dictionary["pFile_evolution_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var index = str.IndexOf("[SPECIES_" + pokemonSpecie.Replace(" ", "_"));
            var index2 = 0;
            var preStr = "";
            var postStr = "";

            if (index >= 0)
            {
                if (totalEvolutions > 0)
                {
                    index = index - 4;
                    index2 = str.IndexOf("}},", index) + 2;
                    preStr = str.Substring(0, index);
                    postStr = str.Substring(index2 + 1);
                    str = preStr + finalString + postStr;
                }
                else if (totalEvolutions == 0)
                {
                    index = index - 4;
                    index2 = str.IndexOf("}},", index) + 2;
                    preStr = str.Substring(0, index);
                    postStr = str.Substring(index2 + 1);
                    str = preStr + postStr;
                }
            }
            else
            {
                if (totalEvolutions > 0)
                {
                    index = 0;
                    index2 = str.IndexOf("};", 0) - 1;
                    preStr = str.Substring(0, index2);
                    postStr = str.Substring(index2);
                    str = preStr + "\n" + finalString + "\n" + postStr;
                }
            }

            data["pFile_evolution_h"] = str;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_evolution_h"].ToString(), false);
            sw.WriteLine(data["pFile_evolution_h"]);
            sw.Close();

        }

        private void setMovements()
        {
            var totalMovements = dataGridView1.Rows.Count - 1;
            var pokemonSpecie = cmbInforma_Species.Text;
            var formatPokemonName = (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(pokemonSpecie.ToLower())).Replace(" ", "");
            if (formatPokemonName == "MrMime")
            {
                formatPokemonName = "Mrmime";
            }

            string[] ataque = new string[totalMovements];
            string[] nivel = new string[totalMovements];
            string finalString = null;

            if (totalMovements > 0)
            {
                for (int i = 0; i < totalMovements; i++)
                {
                    ataque[i] = dataGridView1.Rows[i].Cells[0].Value.ToString().Replace(" ", "_");
                    nivel[i] = dataGridView1.Rows[i].Cells[1].Value.ToString().Replace(" ", "_");
                }

                finalString = "const u16 g" + formatPokemonName + "LevelUpLearnset[] = {\n";
                for (int i = 0; i < totalMovements; i++)
                {
                    if (nivel[i].Length == 1)
                    {
                        nivel[i] = " " + nivel[i];
                    }
                    finalString += "    LEVEL_UP_MOVE(" + nivel[i] + ", MOVE_" + ataque[i] + "),\n";
                    //    LEVEL_UP_MOVE( 1, MOVE_TACKLE),
                }
                finalString += "    LEVEL_UP_END\n};";
            }
            else
            {
                finalString = "";
            }

            //richTextBox1.Text = finalString;

            string str = null;

            StreamReader sr = new StreamReader(dictionary["pFile_level_up_learnsets_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var index = str.IndexOf("const u16 g" + formatPokemonName, 0);
            var index2 = str.IndexOf("};", index) + 1;
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index2 + 1);
            str = preStr + finalString + postStr;
            data["pFile_level_up_learnsets_h"] = str;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_level_up_learnsets_h"].ToString(), false);
            sw.WriteLine(data["pFile_level_up_learnsets_h"]);
            sw.Close();
        }

        private void setMTMO()
        {
            var totalMTMO = dataGridView3.Rows.Count - 1;
            var pokemonSpecie = (cmbInforma_Species.Text).Replace(" ", "_");

            string[] MTMO = new string[totalMTMO];
            string finalString = null;

            if (totalMTMO > 0)
            {
                finalString = "    [SPECIES_" + pokemonSpecie + "]";
                var totalSpaces = 12 - pokemonSpecie.Length;

                for (int i = 0; i < totalSpaces; i++)
                {
                    finalString += " ";
                }

                finalString += "= TMHM_LEARNSET(TMHM(";

                for (int i = 0; i < totalMTMO; i++)
                {
                    MTMO[i] = dataGridView3.Rows[i].Cells[0].Value.ToString().Replace(" ", "_");
                    if (i == (totalMTMO - 1))
                    {
                        finalString += MTMO[i] + ")),";
                    }
                    else
                    {
                        finalString += MTMO[i] + ")\n                                        | TMHM(";
                    }
                }
            }
            else
            {

                //[SPECIES_CATERPIE]    = TMHM_LEARNSET(0),
                finalString = "    [SPECIES_" + pokemonSpecie + "]";
                var totalSpaces = 12 - pokemonSpecie.Length;

                for (int i = 0; i < totalSpaces; i++)
                {
                    finalString += " ";
                }
                finalString += "= TMHM_LEARNSET(0),";
            }

            //richTextBox1.Text = finalString;

            string str = null;

            StreamReader sr = new StreamReader(dictionary["pFile_tmhm_learnsets_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var index = str.IndexOf("    [SPECIES_" + pokemonSpecie, 0);
            var index2 = str.IndexOf("),", index) + 3;
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index2);
            str = preStr + finalString + "\n" + postStr;
            data["pFile_tmhm_learnsets_h"] = str;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_tmhm_learnsets_h"].ToString(), false);
            sw.WriteLine(data["pFile_tmhm_learnsets_h"]);
            sw.Close();
        }

        private void setDexDescription()
        {
            //PAGE 1
            var dexDescriptionOne = (descripcionUno.Text);
            var dexFormatOne = "";
            var spacesAmountOne = dexDescriptionOne.Count(f => f == ' ');
            int[] spaceIndexOne = new int[spacesAmountOne];

            //PAGE 2
            var dexDescriptionTwo = (descripcionDos.Text);
            var dexFormatTwo = "";
            var spacesAmountTwo = dexDescriptionTwo.Count(f => f == ' ');
            int[] spaceIndexTwo = new int[spacesAmountTwo];

            var lastSpace = 0;
            int descriptionIndex = 0;
            int maxLengthPerLine = 39;
            int reducedAmount = 0;
            int rowCounter = 0;
            var pokemonName = "";
            var finalString = "";



            for (int i = 0; i < spacesAmountOne; i++)
            {
                spaceIndexOne[i] = dexDescriptionOne.IndexOf(" ", descriptionIndex);
                descriptionIndex = dexDescriptionOne.IndexOf(" ", descriptionIndex) + 1;
            }

            descriptionIndex = 0;

            for (int i = 0; i < spacesAmountTwo; i++)
            {
                spaceIndexTwo[i] = dexDescriptionTwo.IndexOf(" ", descriptionIndex);
                descriptionIndex = dexDescriptionTwo.IndexOf(" ", descriptionIndex) + 1;
            }

            for (int i = 0; i < spacesAmountOne; i++)
            {
                //MessageBox.Show((spaceIndex[i] - reducedAmount) + "\n" + maxLengthPerLine);
                if ((spaceIndexOne[i] - reducedAmount) > maxLengthPerLine)
                {
                    dexFormatOne += "  \"" + dexDescriptionOne.Substring(reducedAmount, lastSpace - reducedAmount) + "\\n\"\n";
                    reducedAmount += lastSpace - reducedAmount + 1;
                    rowCounter++;
                }
                if (i == spacesAmountOne - 1)
                {
                    dexFormatOne += "  \"" + dexDescriptionOne.Substring(reducedAmount) + "\"";
                }
                lastSpace = (spaceIndexOne[i]);
            }

            //MessageBox.Show(dexFormatOne + "\n" + rowCounter.ToString());


            if (rowCounter <= 2)
            {

                lastSpace = 0;
                reducedAmount = 0;
                rowCounter = 0;

                for (int i = 0; i < spacesAmountTwo; i++)
                {
                    if ((spaceIndexTwo[i] - reducedAmount) > maxLengthPerLine)
                    {
                        dexFormatTwo += "  \"" + dexDescriptionTwo.Substring(reducedAmount, lastSpace - reducedAmount) + "\\n\"\n";
                        reducedAmount += lastSpace - reducedAmount + 1;
                        rowCounter++;
                    }
                    if (i == spacesAmountTwo - 1)
                    {
                        dexFormatTwo += "  \"" + dexDescriptionTwo.Substring(reducedAmount) + "\"";
                    }
                    lastSpace = (spaceIndexTwo[i]);
                }

                if (rowCounter <= 2)
                {
                    pokemonName = cmbInforma_Species.Text;
                    pokemonName = (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(pokemonName.ToLower())).Replace(" ", "");
                    if (pokemonName == "MrMime") { pokemonName = "Mrmime"; }

                    finalString = "static const u8 DexDescription_" + pokemonName + "_1[] = _(\n"
                        + dexFormatOne + ");" + "\nstatic const u8 DexDescription_" + pokemonName + "_2[] = _(\n"
                        + dexFormatTwo + ");\n";

                    string str = null;

                    StreamReader sr = new StreamReader(dictionary["pFile_pokedex_entries_en_h"].ToString());
                    str = sr.ReadToEnd();
                    sr.Close();
                    var index = 0;
                    var totalToChange = Regex.Matches(str, "static const u8 DexDescription_" + pokemonName + "_1").Cast<Match>().Count();
                    for (int i = 0; i < totalToChange; i++)
                    {
                        index = str.IndexOf("static const u8 DexDescription_" + pokemonName + "_1", index + 1);
                        var index2 = str.IndexOf(");", index) + 2;
                        index2 = str.IndexOf(");", index2) + 2;
                        var preStr = str.Substring(0, index);
                        var postStr = str.Substring(index2 + 1);
                        str = preStr + finalString + postStr;
                    }

                    data["pFile_pokedex_entries_en_h"] = str;

                    //richTextBox1.Text = str;

                    StreamWriter sw = new StreamWriter(dictionary["pFile_pokedex_entries_en_h"].ToString(), false);
                    sw.WriteLine(data["pFile_pokedex_entries_en_h"]);
                    sw.Close();

                }
                else
                {
                    MessageBox.Show("La página dos de la descripción excede la longitud máxima");
                }

            }
            else
            {
                MessageBox.Show("La página uno de la descripción excede la longitud máxima");
            }
        }

        private void setDexData()
        {
            var categoryName = categoriaPokemon.Text;
            var height = altura.Text;
            var weight = peso.Text;
            var pokemonScale = escalaPokemon.Text;
            var pokemonOffset = offsetPokemon.Text;
            var trainerScale = escalaEntrenador.Text;
            var trainerOffset = offsetEntrenador.Text;
            var pokemonSpecie = cmbInforma_Species.Text;
            var formatPokemonName = (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(pokemonSpecie.ToLower())).Replace(" ", "");
            if (formatPokemonName == "MrMime")
            {
                formatPokemonName = "Mrmime";
            }

            var finalString = "    {  //" + formatPokemonName +
                "\n        .categoryName = _(\"" + categoryName + "\"),\n" +
                "        .height = " + height + ",\n" +
                "        .weight = " + weight + ",\n" +
                "        .descriptionPage1 = DexDescription_" + formatPokemonName + "_1,\n" +
                "        .descriptionPage2 = DexDescription_" + formatPokemonName + "_2,\n" +
                "        .pokemonScale = " + pokemonScale + ",\n" +
                "        .pokemonOffset = " + pokemonOffset + ",\n" +
                "        .trainerScale = " + trainerScale + ",\n" +
                "        .trainerOffset = " + trainerOffset + ",\n    },\n";

            //richTextBox1.Text = finalString;

            string str = null;

            StreamReader sr = new StreamReader(dictionary["pFile_pokedex_entries_en_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var index = str.IndexOf("descriptionPage1 = DexDescription_" + formatPokemonName + "_1", 0);
            index = str.LastIndexOf("{", index) - 4;
            var index2 = str.IndexOf("},", index) + 2;
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index2 + 1);
            str = preStr + finalString + postStr;
            //richTextBox1.Text = str;
            data["pFile_pokedex_entries_en_h"] = str;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_pokedex_entries_en_h"].ToString(), false);
            sw.WriteLine(data["pFile_pokedex_entries_en_h"]);
            sw.Close();

        }

        private void setSpriteData()
        {
            if (pokemonData["backCord"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                setBackCord();
            }
            if (pokemonData["frontCord"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                setFrontCord();
            }
            if (pokemonData["elevate"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                setElevation();
            }
            if (pokemonData["palUsed"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                setPalUsed();
            }
        }

        private void setBackCord()
        {
            string backCord = backY.Value.ToString();
            string firstByte = getSizeValue(pictureBox2.Image);
            if (backCord.Length == 1)
            {
                backCord = " " + backCord;
            }
            switch (firstByte.Length)
            {
                case 1:
                    firstByte = "  " + firstByte;
                    break;
                case 2:
                    firstByte = " " + firstByte;
                    break;
            }
            string finalString = ".byte " + firstByte + ", " + backCord + ", 0, 0";
            //MessageBox.Show(finalString);
            string str = null;

            StreamReader sr = new StreamReader(dictionary["pFile_back_pic_coords_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();
            var index = 0;
            var pokemonIndex = cmbInforma_Species.SelectedIndex;

            for (int i = 0; i <= pokemonIndex; i++)
            {
                index = str.IndexOf(".byte", index + 1);
            }

            var getString = str.Substring(index, str.IndexOf("\n", index) - index);

            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index + getString.Length);
            var resultString = preStr + finalString + postStr;

            //MessageBox.Show(getString);

            data["pFile_back_pic_coords_inc"] = resultString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_back_pic_coords_inc"].ToString(), false);
            sw.WriteLine(data["pFile_back_pic_coords_inc"]);
            sw.Close();
            //MessageBox.Show(finalString);
        }

        private void setFrontCord()
        {
            string frontCord = frontY.Value.ToString();
            string firstByte = getSizeValue(pictureBox3.Image);
            if (frontCord.Length == 1)
            {
                frontCord = " " + frontCord;
            }
            switch (firstByte.Length)
            {
                case 1:
                    firstByte = "  " + firstByte;
                    break;
                case 2:
                    firstByte = " " + firstByte;
                    break;
            }
            string finalString = ".byte " + firstByte + ", " + frontCord + ", 0, 0";



            string str = null;

            StreamReader sr = new StreamReader(dictionary["pFile_front_pic_coords_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();
            var index = 0;
            var pokemonIndex = cmbInforma_Species.SelectedIndex;

            for (int i = 0; i <= pokemonIndex; i++)
            {
                index = str.IndexOf(".byte", index + 1);
            }

            var getString = str.Substring(index, str.IndexOf("\n", index) - index);

            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index + getString.Length);

            var resultString = preStr + finalString + postStr;

            //MessageBox.Show(getString);

            data["pFile_front_pic_coords_inc"] = resultString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_front_pic_coords_inc"].ToString(), false);
            sw.WriteLine(data["pFile_front_pic_coords_inc"]);
            sw.Close();
            //MessageBox.Show(finalString);
        }

        private void setElevation()
        {
            string pokemonName = cmbInforma_Species.Text.ToUpper().Replace(" ", "_");

            string str = null;

            StreamReader sr = new StreamReader(dictionary["pFile_battle_1_c"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var index = str.IndexOf("[SPECIES_" + pokemonName + "]", 0);

            var getString = str.Substring(index, str.IndexOf(",", index) - index);

            var levitation = Levitation.Value.ToString();

            if (levitation.Length == 1)
            {
                levitation = " " + levitation;
            }

            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index + getString.Length);

            var resultString = preStr + "[SPECIES_" + pokemonName + "] = " + levitation + postStr;

            //MessageBox.Show(getString);

            data["pFile_battle_1_c"] = resultString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_battle_1_c"].ToString(), false);
            sw.WriteLine(data["pFile_battle_1_c"]);
            sw.Close();
        }

        private void setPalUsed()
        {
            string palUsed = iconPalette.Text.Replace(" ", "");

            string str = null;

            StreamReader sr = new StreamReader(dictionary["pFile_pokemon_icon_c"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var index = str.IndexOf("const u8 gMonIconPaletteIndices[]", 0);
            var pokemonAmount = Regex.Matches(str, "gMonIcon").Cast<Match>().Count() - 1;
            index = str.IndexOf("{", index);
            for (int i = 0; i <= cmbInforma_Species.SelectedIndex; i++)
            {
                index = str.IndexOf("\n", index) + 1;
            }
            var pokemonFormat = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cmbInforma_Species.Text.ToLower()).Replace(" ", "");
            var newString = "    " + palUsed + ", // " + pokemonFormat + "\n";
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(str.IndexOf("\n", index) + 1);
            var finalString = preStr + newString + postStr;
            //MessageBox.Show(getString);

            data["pFile_pokemon_icon_c"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_pokemon_icon_c"].ToString(), false);
            sw.WriteLine(data["pFile_pokemon_icon_c"]);
            sw.Close();
        }


        private void reloadAllSavedData()
        {
            pokemonData["psBase"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_baseHP.Text;
            pokemonData["ataqueBase"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_baseATK.Text;
            pokemonData["defensaBase"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_baseDEF.Text;
            pokemonData["velocidadBase"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_baseSPEED.Text;
            pokemonData["ataqueEspecialBase"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_baseSPATK.Text;
            pokemonData["defensaEspecialBase"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_baseSPDEF.Text;
            //Guardar tipo 1
            var formatoTipo1 = "TYPE_" + ddlStats_type1.Text;
            formatoTipo1 = formatoTipo1.Replace(" ", "_");
            pokemonData["tipoUno"][cmbInforma_Species.SelectedIndex.ToString()] = formatoTipo1;
            //Recibir tipo 2
            var formatoTipo2 = "TYPE_" + ddlStats_type2.Text;
            formatoTipo2 = formatoTipo2.Replace(" ", "_");
            pokemonData["tipoDos"][cmbInforma_Species.SelectedIndex.ToString()] = formatoTipo2;

            pokemonData["ratioDeCaptura"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_catchRate.Text;
            pokemonData["expBase"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_expYield.Text;
            pokemonData["evsPS"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_evHP.Text;
            pokemonData["evsAtaque"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_evATK.Text;
            pokemonData["evsDefensa"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_evDEF.Text;
            pokemonData["evsVelocidad"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_evSPEED.Text;
            pokemonData["evsAtaqueEspecial"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_evSPATK.Text;
            pokemonData["evsDefensaEspecial"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_evSPDEF.Text;
            //Recibir objeto 1
            var formatoObjeto1 = "ITEM_" + ddlStats_item1.Text;
            formatoObjeto1 = formatoObjeto1.Replace(@" ", "_");
            pokemonData["objetoUno"][cmbInforma_Species.SelectedIndex.ToString()] = formatoObjeto1;
            //Recibir objeto 2
            var formatoObjeto2 = "ITEM_" + ddlStats_item2.Text;
            formatoObjeto2 = formatoObjeto2.Replace(@" ", "_");
            pokemonData["objetoDos"][cmbInforma_Species.SelectedIndex.ToString()] = formatoObjeto2;

            pokemonData["ratioGenero"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_genderRatio.Text;
            pokemonData["tieneGenero"][cmbInforma_Species.SelectedIndex.ToString()] = chkStats_gender.Checked.ToString();
            pokemonData["ciclosHuevo"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_eggCycles.Text;
            pokemonData["amistadBase"][cmbInforma_Species.SelectedIndex.ToString()] = txtStats_friendship.Text;
            pokemonData["crecimiento"][cmbInforma_Species.SelectedIndex.ToString()] = "GROWTH_" + (ddlStats_growthRate.Text).Replace(" ", "_");
            pokemonData["grupoHuevoUno"][cmbInforma_Species.SelectedIndex.ToString()] = "EGG_GROUP_" + (ddlStats_eggGroup1.Text).Replace(" ", "_");
            pokemonData["grupoHuevoDos"][cmbInforma_Species.SelectedIndex.ToString()] = "EGG_GROUP_" + (ddlStats_eggGroup2.Text).Replace(" ", "_");
            pokemonData["habilidadUno"][cmbInforma_Species.SelectedIndex.ToString()] = "ABILITY_" + (ddlStats_ability1.Text).Replace(" ", "_");
            pokemonData["habilidadDos"][cmbInforma_Species.SelectedIndex.ToString()] = "ABILITY_" + (ddlStats_ability2.Text).Replace(" ", "_");
            pokemonData["probabilidadHuidaSafari"][cmbInforma_Species.SelectedIndex.ToString()] = ddlStats_safariFleeRate.Text;
            pokemonData["colorCuerpo"][cmbInforma_Species.SelectedIndex.ToString()] = "BODY_COLOR_" + (ddlStats_bodyColor.Text).Replace(" ", "_");
            pokemonData["pokemonName"][cmbInforma_Species.SelectedIndex.ToString()] = POKEMON_NAME.Text.Replace(" ", "_");
            //EVOLUTION
            var evosAmount = this.dataGridView2.Rows.Count - 1;
            string method;
            string argument;
            string evolution;

            for (int i = 0; i < evosAmount; i++)
            {
                method = "EVO_" + (this.dataGridView2.Rows[i].Cells[0].Value).ToString().Replace(" ", "_");
                argument = (this.dataGridView2.Rows[i].Cells[1].Value).ToString();
                evolution = "SPECIES_" + (this.dataGridView2.Rows[i].Cells[2].Value).ToString().Replace(" ", "_");
                //MessageBox.Show(method + "\n" + argument + "\n" + evolution);
                evolutionData[cmbInforma_Species.SelectedIndex.ToString() + "_" + i.ToString()] = Tuple.Create(evosAmount.ToString(), method, argument, evolution);
            }
            //MOVEMENTS
            var moveAmount = this.dataGridView1.Rows.Count - 1;
            string level;
            string movement;

            for (int i = 0; i < moveAmount; i++)
            {
                level = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                movement = "MOVE_" + this.dataGridView1.Rows[i].Cells[0].Value.ToString().Replace(" ", "_");
                moveData[cmbInforma_Species.SelectedIndex.ToString() + "_" + i.ToString()] = Tuple.Create(moveAmount.ToString(), level, movement);
            }
            //MT/MO
            var mtmoAmount = this.dataGridView3.Rows.Count - 1;
            string mtmo;

            for (int i = 0; i < mtmoAmount; i++)
            {
                mtmo = this.dataGridView3.Rows[i].Cells[0].Value.ToString().Replace(" ", "_");
                mtmoData[cmbInforma_Species.SelectedIndex.ToString() + "_" + i.ToString()] = Tuple.Create(mtmoAmount.ToString(), mtmo);
            }
            //POKEDEX INFORMATION
            pokemonData["pokedexPageOne"][cmbInforma_Species.SelectedIndex.ToString()] = descripcionUno.Text;
            pokemonData["pokedexPageTwo"][cmbInforma_Species.SelectedIndex.ToString()] = descripcionDos.Text;
            pokemonData["categoriaPokemon"][cmbInforma_Species.SelectedIndex.ToString()] = categoriaPokemon.Text;
            pokemonData["altura"][cmbInforma_Species.SelectedIndex.ToString()] = altura.Text;
            pokemonData["peso"][cmbInforma_Species.SelectedIndex.ToString()] = peso.Text;
            pokemonData["escalaPokemon"][cmbInforma_Species.SelectedIndex.ToString()] = escalaPokemon.Text;
            pokemonData["offsetPokemon"][cmbInforma_Species.SelectedIndex.ToString()] = offsetPokemon.Text;
            pokemonData["escalaEntrenador"][cmbInforma_Species.SelectedIndex.ToString()] = escalaEntrenador.Text;
            pokemonData["offsetEntrenador"][cmbInforma_Species.SelectedIndex.ToString()] = offsetEntrenador.Text;
            pokemonData["palUsed"][cmbInforma_Species.SelectedIndex.ToString()] = iconPalette.Text;

            //SPRITES
            if (pokemonData["frontCord"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                pokemonData["frontCord"][cmbInforma_Species.SelectedIndex.ToString()] = frontY.Value.ToString();
            }
            if (pokemonData["backCord"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                pokemonData["backCord"][cmbInforma_Species.SelectedIndex.ToString()] = backY.Value.ToString();
            }
            if (pokemonData["elevate"].ContainsKey(cmbInforma_Species.SelectedIndex.ToString()))
            {
                pokemonData["elevate"][cmbInforma_Species.SelectedIndex.ToString()] = Levitation.Value.ToString();
            }
        }

        private void setSpritePosition()
        {
            pictureBox2.Location = new Point(40, 48 + Int32.Parse((backY.Value).ToString()));
            pictureBox3.Location = new Point(144, 8 + Int32.Parse((frontY.Value).ToString()) - Int32.Parse((Levitation.Value).ToString()));
            if (Levitation.Value > 0)
            {
                fondo.Image = Properties.Resources.bgSombra;
            }
            else
            {
                fondo.Image = Properties.Resources.bg1;
            }
        }


        // * * * * * * * * * * * //
        // Generar nuevo Pokémon //
        // * * * * * * * * * * * //

        private void generarPokemon()
        {
            DialogResult dr = MessageBox.Show("¿Inserción masiva?",
                      "Modo de inserción", MessageBoxButtons.YesNo);

            switch (dr)
            {
                case DialogResult.Yes:
                    insercionMasiva();
                    cmbInforma_Species.SelectedIndex = cmbInforma_Species.Items.Count - 1;
                    break;
                case DialogResult.No:
                    insercionIndividual();
                    cmbInforma_Species.SelectedIndex = cmbInforma_Species.Items.Count - 1;
                    break;
            }
        }

        private string insercionIndividual()
        {
            NombrePokemon f = new NombrePokemon();
            if (f.ShowDialog() == DialogResult.OK)
            {
                minusPokemonName = (f.PkmnName).ToLower();
                mayusPokemonName = (f.PkmnName).ToUpper().Replace(" ", "_");
                firstMayusPokemonName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(f.PkmnName.ToLower()).Replace(" ", "");
                var checkPokemon = mayusPokemonName;
                if (checkPokemon == "NIDORAN_M")
                {
                    checkPokemon = "NIDORAN♂";
                }
                else if (checkPokemon == "NIDORAN_F")
                {
                    checkPokemon = "NIDORAN♀";
                }
                if (pokemonData["pokemonName"].Any(tr => tr.Value.Equals(checkPokemon, StringComparison.CurrentCultureIgnoreCase)) == false)
                {
                    species_h_set();
                    global_h_set();
                    base_stats_h_set();
                    cry_ids_h_set();
                    level_up_learnset_pointers_set();
                    level_up_learnset_set();
                    tmhm_learnset_set();
                    pokedex_orders_set();
                    pokedex_entries_set();
                    species_names_set();
                    pokedex_c_set();
                    pokedex_h_set();
                    pokemon_1_set();
                    pokemon_icon_set();
                    graphics_set();
                    back_pic_table_set();
                    front_pic_table_set();
                    graphics_inc_set();
                    palette_table_set();
                    shiny_palette_table_set();
                    back_pick_coords_set();
                    front_pick_coords_set();
                    battle_1_set();
                    direct_sound_data_set();
                    data_reset();
                    return "true";
                }
                else
                {
                    MessageBox.Show("Este pokémon ya existe, prueba con otro nombre.");
                    return "false";
                }

            }
            else
            {
                return "break";
            }

        }

        private void insercionMasiva()
        {
            Datos f = new Datos();
            if (f.ShowDialog() == DialogResult.OK)
            {
                var newPokes = f.PkmnAmount;
                for (int i = 0; i < newPokes; i++)
                {
                    var result = insercionIndividual();
                    if (result == "false") { i--; } else if (result == "break") { break; }
                }
            }
        }

        private void species_h_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_species_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();
            var index = 0;
            var lastIndex = index;
            string preStr = null;
            string postStr = null;
            int indexEgg = str.IndexOf("#define SPECIES_EGG", index);

            //Change SPECIES

            int eggValue = Int32.Parse(str.Substring((indexEgg + 20), str.IndexOf("\n", indexEgg) - (indexEgg + 20)));
            eggValue--;
            index = str.IndexOf(eggValue.ToString(), index) + eggValue.ToString().Length;
            eggValue++;
            preStr = str.Substring(0, index);
            postStr = str.Substring(str.IndexOf("#define NUM_SPECIES SPECIES_EGG", 0) - 1);
            var afterValue = resetAfterValues(eggValue);
            //richTextBox1.Text = preStr + "\n#define SPECIES_" + mayusPokemonName + " " + (eggValue) + afterValue + postStr;
            str = preStr + "\n#define SPECIES_" + mayusPokemonName + " " + (eggValue) + afterValue + postStr;

            //Change NATIONAL_DEX

            index = 0;
            var unownBIndex = str.IndexOf("#define NATIONAL_DEX_OLD_UNOWN_B", 0) + 33;
            var unownValue = str.Substring(unownBIndex, (str.IndexOf("\n", unownBIndex)) - unownBIndex);

            var secondString = "#define NATIONAL_DEX_" + mayusPokemonName + " " + unownValue;
            var nationalAmount = Regex.Matches(str, "#define NATIONAL_DEX_").Cast<Match>().Count() - 25;
            int plusValue = Int32.Parse(unownValue);
            string highSecondString = null;

            index = str.IndexOf("#define NATIONAL_DEX_CELEBI", 0);

            for (int i = 0; i < 25; i++)
            {
                var unownWorkString = str.Substring(str.IndexOf("#define NATIONAL_DEX_", index + 1), str.IndexOf(" ", str.IndexOf("NATIONAL_DEX", str.IndexOf("#define", index + 1))) - str.IndexOf("#define NATIONAL_DEX_", index + 1));
                index = str.IndexOf("#define NATIONAL_DEX_", index + 1);
                plusValue++;
                highSecondString += unownWorkString + " " + plusValue + "\n";
            }
            index = 0;
            preStr = str.Substring(0, str.IndexOf("#define NATIONAL_DEX_OLD_UNOWN_B", index));
            postStr = str.Substring(str.IndexOf("\n", str.IndexOf("#define NATIONAL_DEX_OLD_UNOWN_Z", index)) + 1);
            str = preStr + highSecondString + postStr;

            preStr = str.Substring(0, str.IndexOf("\n", str.LastIndexOf("#define NATIONAL_DEX_")));
            postStr = str.Substring(str.IndexOf("\n", str.LastIndexOf("#define NATIONAL_DEX_")));
            str = preStr + "\n" + secondString + postStr;
            //richTextBox1.Text = str;

            //Change HOENN_DEX

            var unownBIndex2 = str.IndexOf("#define HOENN_DEX_OLD_UNOWN_B", 0) + 30;
            var unownValue2 = str.Substring(unownBIndex2, (str.IndexOf("\n", unownBIndex2)) - unownBIndex2);

            var thirdString = "#define HOENN_DEX_" + mayusPokemonName + " " + unownValue2;
            var hoennAmount = Regex.Matches(str, "#define HOENN_DEX_").Cast<Match>().Count() - 25;
            plusValue = Int32.Parse(unownValue2);
            string highThirdString = null;

            index = str.IndexOf("#define HOENN_DEX_OLD_UNOWN_B", 0) - 1;

            for (int i = 0; i < 25; i++)
            {
                var unownWorkString = str.Substring(str.IndexOf("#define HOENN_DEX_", index + 1), str.IndexOf(" ", str.IndexOf("HOENN_DEX_", str.IndexOf("#define", index + 1))) - str.IndexOf("#define HOENN_DEX_", index + 1));
                index = str.IndexOf("#define HOENN_DEX_", index + 1);
                plusValue++;
                highThirdString += unownWorkString + " " + plusValue + "\n";
            }
            index = 0;
            preStr = str.Substring(0, str.IndexOf("#define HOENN_DEX_OLD_UNOWN_B", index) - 2);
            postStr = str.Substring(str.IndexOf("\n", str.IndexOf("#define HOENN_DEX_OLD_UNOWN_Z", index)) + 1);
            str = preStr + "\n" + thirdString + "\n\n" + highThirdString + postStr;
            //richTextBox1.Text = str;

            data["pFile_species_h"] = str;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_species_h"].ToString(), false);
            sw.WriteLine(data["pFile_species_h"]);
            sw.Close();

        }

        private void global_h_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_global_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var actualNumber = Int32.Parse(str.Substring((str.IndexOf("#define POKEMON_SLOTS_NUMBER", 0) + 29), str.IndexOf("\n", str.IndexOf("#define POKEMON_SLOTS_NUMBER", 0) + 1) - (str.IndexOf("#define POKEMON_SLOTS_NUMBER", 0) + 29)));
            var preStr = str.Substring(0, str.IndexOf("#define POKEMON_SLOTS_NUMBER", 0));
            var postStr = str.Substring(str.IndexOf("\n", str.IndexOf("#define POKEMON_SLOTS_NUMBER", 0)));
            actualNumber++;
            var finalString = preStr + "#define POKEMON_SLOTS_NUMBER " + actualNumber + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_global_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_global_h"].ToString(), false);
            sw.WriteLine(data["pFile_global_h"]);
            sw.Close();
        }

        private void base_stats_h_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_base_stats_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var index = str.IndexOf("};", 0) - 1;
            var baseStatsString = ",\n\n    [SPECIES_" + mayusPokemonName + "] =" +
                "\n    {" +
                "\n        .baseHP        = 0," +
                "\n        .baseAttack    = 0," +
                "\n        .baseDefense   = 0," +
                "\n        .baseSpeed     = 0," +
                "\n        .baseSpAttack  = 0," +
                "\n        .baseSpDefense = 0," +
                "\n        .type1 = TYPE_NORMAL," +
                "\n        .type2 = TYPE_NORMAL," +
                "\n        .catchRate = 0," +
                "\n        .expYield = 0," +
                "\n        .evYield_HP        = 0," +
                "\n        .evYield_Attack    = 0," +
                "\n        .evYield_Defense   = 0," +
                "\n        .evYield_Speed     = 0," +
                "\n        .evYield_SpAttack  = 0," +
                "\n        .evYield_SpDefense = 0," +
                "\n        .item1 = ITEM_NONE," +
                "\n        .item2 = ITEM_NONE," +
                "\n        .genderRatio = PERCENT_FEMALE(50)," +
                "\n        .eggCycles = 0," +
                "\n        .friendship = 0," +
                "\n        .growthRate = GROWTH_MEDIUM_FAST," +
                "\n        .eggGroup1 = EGG_GROUP_FIELD," +
                "\n        .eggGroup2 = EGG_GROUP_FIELD," +
                "\n        .ability1 = ABILITY_NONE," +
                "\n        .ability2 = ABILITY_NONE," +
                "\n        .safariZoneFleeRate = 0," +
                "\n        .bodyColor = BODY_COLOR_GRAY," +
                "\n        .noFlip = FALSE," +
                "\n    }";

            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + baseStatsString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_base_stats_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_base_stats_h"].ToString(), false);
            sw.WriteLine(data["pFile_base_stats_h"]);
            sw.Close();
        }

        private void cry_ids_h_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_cry_ids_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();
            var valueIndex = str.IndexOf("\n", str.LastIndexOf(",")) + 5;
            //MessageBox.Show(str.Substring(valueIndex, str.IndexOf(" ", valueIndex) - valueIndex));
            var pokemonValue = Int32.Parse(str.Substring(valueIndex, str.IndexOf(" ", valueIndex) - valueIndex));
            pokemonValue++;
            var index = str.IndexOf(" ", str.IndexOf("\n", str.LastIndexOf(",")) + 5);
            var finalString = str.Substring(0, index) + "," + str.Substring(index + 1);
            str = finalString;

            index = str.IndexOf("};") - 1;
            var newString = "\n    " + pokemonValue + "  // " + mayusPokemonName;
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_cry_ids_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_cry_ids_h"].ToString(), false);
            sw.WriteLine(data["pFile_cry_ids_h"]);
            sw.Close();
        }

        private void level_up_learnset_pointers_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_level_up_learnset_pointers_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "};";
            int index = str.LastIndexOf(toSearch) - 1;
            //
            string newString = ",\n    g" + firstMayusPokemonName + "LevelUpLearnset";
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_level_up_learnset_pointers_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_level_up_learnset_pointers_h"].ToString(), false);
            sw.WriteLine(data["pFile_level_up_learnset_pointers_h"]);
            sw.Close();

        }

        private void level_up_learnset_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_level_up_learnsets_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "};";
            int index = str.LastIndexOf(toSearch) + 2;
            //
            string newString = "\n\nconst u16 g" + firstMayusPokemonName + "LevelUpLearnset[] = {\n" +
                "    LEVEL_UP_MOVE( 1, MOVE_TACKLE),\n" +
                "    LEVEL_UP_END\n};";
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_level_up_learnsets_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_level_up_learnsets_h"].ToString(), false);
            sw.WriteLine(data["pFile_level_up_learnsets_h"]);
            sw.Close();
        }

        private void tmhm_learnset_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_tmhm_learnsets_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "};";
            int index = str.LastIndexOf(toSearch) - 2;
            //"    [SPECIES_CATERPIE]    = TMHM_LEARNSET(0),"
            string newString = "\n\n    [SPECIES_" + mayusPokemonName + "]";
            for (int i = 0; i < (12 - mayusPokemonName.Length); i++)
            {
                newString += " ";
            }
            newString += "= TMHM_LEARNSET(0),";
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_tmhm_learnsets_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_tmhm_learnsets_h"].ToString(), false);
            sw.WriteLine(data["pFile_tmhm_learnsets_h"]);
            sw.Close();
        }

        private void pokedex_orders_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_pokedex_orders_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();
            String[] pokeNames = new string[(pokemonData["pokemonName"].Count + 1)];
            for (int i = 0; i < pokemonData["pokemonName"].Count; i++)
            {
                pokeNames[i] = pokemonData["pokemonName"][(i + 1).ToString()];
            }
            pokeNames[pokemonData["pokemonName"].Count] = mayusPokemonName;
            var sortedNames = pokeNames.OrderBy(n => n);
            string[] pokeNameOrdered = sortedNames.ToArray();
            var newPokeIndex = Array.FindIndex(pokeNameOrdered, row => row.ToString() == mayusPokemonName);
            if (pokeNameOrdered[newPokeIndex + 1].ToString() == "MR. MIME") { pokeNameOrdered[newPokeIndex + 1] = "MR MIME"; }
            var index = str.IndexOf("NATIONAL_DEX_" + pokeNameOrdered[newPokeIndex + 1].ToString().Replace(" ", "_"), 0);
            //MessageBox.Show(("NATIONAL_DEX_" + pokeNameOrdered[newPokeIndex + 1]).Replace(" ", "n"));
            var newString = "NATIONAL_DEX_" + mayusPokemonName;
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + ",\n    " + postStr;
            str = finalString;
            index = str.IndexOf("static const u16 gPokedexOrder_Weight[] =", 0);
            index = str.IndexOf("{", index) + 1;
            preStr = str.Substring(0, index);
            postStr = str.Substring(index);
            finalString = preStr + "\n    " + newString + "," + postStr;
            str = finalString;
            index = str.IndexOf("static const u16 gPokedexOrder_Height[] =", 0);
            index = str.IndexOf("{", index) + 1;
            preStr = str.Substring(0, index);
            postStr = str.Substring(index);
            finalString = preStr + "\n    " + newString + "," + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_pokedex_orders_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_pokedex_orders_h"].ToString(), false);
            sw.WriteLine(data["pFile_pokedex_orders_h"]);
            sw.Close();
        }

        private void pokedex_entries_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_pokedex_entries_en_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "const struct PokedexEntry gPokedexEntries[] =";
            int index = str.LastIndexOf(toSearch) - 2;

            string newString = "\n\nstatic const u8 DexDescription_" + firstMayusPokemonName + "_1[] = _(" +
                "\n  \"\");" +
                "\nstatic const u8 DexDescription_" + firstMayusPokemonName + "_2[] = _(" +
                "\n  \"\");";
            string preStr = str.Substring(0, index);
            string postStr = str.Substring(index);
            string finalString = preStr + newString + postStr;
            str = finalString;
            //richTextBox1.Text = finalString;

            index = str.LastIndexOf("};") - 1;
            newString = "    {  //" + firstMayusPokemonName +
                "\n        .categoryName = _(\"NULL\")," +
                "\n        .height = 0," +
                "\n        .weight = 0," +
                "\n        .descriptionPage1 = DexDescription_" + firstMayusPokemonName + "_1," +
                "\n        .descriptionPage2 = DexDescription_" + firstMayusPokemonName + "_2," +
                "\n        .pokemonScale = 0," +
                "\n        .pokemonOffset = 0," +
                "\n        .trainerScale = 0," +
                "\n        .trainerOffset = 0," +
                "\n    },";
            preStr = str.Substring(0, index);
            postStr = str.Substring(index);
            finalString = preStr + "\n" + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_pokedex_entries_en_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_pokedex_entries_en_h"].ToString(), false);
            sw.WriteLine(data["pFile_pokedex_entries_en_h"]);
            sw.Close();
        }

        private void species_names_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_species_names_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "};";
            int index = str.LastIndexOf(toSearch) - 1;
            string newString = "\n    [SPECIES_" + mayusPokemonName + "] = _(\"" + mayusPokemonName + "\"),";
            string preStr = str.Substring(0, index);
            string postStr = str.Substring(index);
            string finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_species_names_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_species_names_h"].ToString(), false);
            sw.WriteLine(data["pFile_species_names_h"]);
            sw.Close();
        }

        private void pokedex_c_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_pokedex_c"].ToString());
            str = sr.ReadToEnd();
            sr.Close();
            int index;
            string newString;
            string preStr;
            string postStr;
            string finalString;

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "#define NATIONAL_DEX_COUNT";
            if (str.IndexOf(toSearch, 0) != -1)
            {
                index = str.IndexOf(toSearch, 0) + 27;
                int newInt = Int32.Parse(str.Substring(index, (str.IndexOf("\n", index) - index)));
                newInt++;
                newString = newInt.ToString();
                preStr = str.Substring(0, index);
                index = str.IndexOf("\n", index);
                postStr = str.Substring(index);
                finalString = preStr + newString + postStr;
                str = finalString;
            }
            index = str.LastIndexOf("gMonFootprint_Bulbasaur,") - 5;
            newString = "\n    gMonFootprint_" + firstMayusPokemonName + ",";
            preStr = str.Substring(0, index);
            postStr = str.Substring(index);
            finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_pokedex_c"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_pokedex_c"].ToString(), false);
            sw.WriteLine(data["pFile_pokedex_c"]);
            sw.Close();
        }

        private void pokedex_h_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_pokedex_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "#define NATIONAL_DEX_COUNT";
            int index = str.IndexOf(toSearch, 0) + 27;
            int newInt = Int32.Parse(str.Substring(index, (str.IndexOf("\n", index) - index)));
            newInt++;
            var newString = newInt.ToString();
            var preStr = str.Substring(0, index);
            index = str.IndexOf("\n", index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_pokedex_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_pokedex_h"].ToString(), false);
            sw.WriteLine(data["pFile_pokedex_h"]);
            sw.Close();
        }

        private void pokemon_1_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_pokemon_1_c"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "gSpeciesToHoennPokedexNum";
            int index = str.IndexOf(toSearch);
            index = str.IndexOf("HOENN_DEX_OLD_UNOWN_B,", index) - 2;
            string newString = "\n\tHOENN_DEX_" + mayusPokemonName + ",";
            for (int i = 0; i < (17 - mayusPokemonName.Length); i++)
            {
                newString += " ";
            }
            newString += "// SPECIES_" + mayusPokemonName;
            string preStr = str.Substring(0, index);
            string postStr = str.Substring(index);
            string finalString = preStr + newString + postStr;
            str = finalString;

            index = str.IndexOf("const u16 gSpeciesToNationalPokedexNum[]", 0);
            var index2 = str.IndexOf("};", index) + 2;
            var workString = str.Substring(index, index2 - index);
            var workIndex = workString.LastIndexOf("NATIONAL_DEX_");
            var workHalfString = workString.Substring(workIndex, workString.IndexOf(" ", workIndex) - workIndex) + ",";
            var preWorkStr = workString.Substring(0, workIndex);
            var postWorkStr = workString.Substring(workString.IndexOf(" ", workIndex));
            var finalWorkStr = preWorkStr + workHalfString + postWorkStr;
            preStr = str.Substring(0, index);
            postStr = str.Substring(index2);
            finalString = preStr + finalWorkStr + postStr;
            str = finalString;

            index = str.IndexOf("};", index) - 1;
            newString = "\n\tNATIONAL_DEX_" + mayusPokemonName;
            for (int i = 0; i < (19 - mayusPokemonName.Length); i++)
            {
                newString += " ";
            }
            newString += "// SPECIES_" + mayusPokemonName;
            preStr = str.Substring(0, index);
            postStr = str.Substring(index);
            finalString = preStr + newString + postStr;
            str = finalString;

            index = str.IndexOf("gHoennToNationalOrder", 0);
            index = str.IndexOf("NATIONAL_DEX_OLD_UNOWN_B,", index) - 2;
            newString = "\n\tNATIONAL_DEX_" + mayusPokemonName + ",";
            for (int i = 0; i < (14 - mayusPokemonName.Length); i++)
            {
                newString += " ";
            }
            newString += "// SPECIES_" + mayusPokemonName;
            preStr = str.Substring(0, index);
            postStr = str.Substring(index);
            finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_pokemon_1_c"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_pokemon_1_c"].ToString(), false);
            sw.WriteLine(data["pFile_pokemon_1_c"]);
            sw.Close();
        }

        private void pokemon_icon_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_pokemon_icon_c"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "const u8 * const gMonIconTable[]";
            int index = str.LastIndexOf(toSearch) - 1;
            index = str.IndexOf("    gMonIcon_Egg,", index) - 1;
            var newString = "\n    gMonIcon_" + firstMayusPokemonName + ",";
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            str = finalString;

            index = str.IndexOf("gMonIconPaletteIndices", index);
            index = str.IndexOf("    1, // Egg", index) - 1;
            newString = "\n    0, // " + firstMayusPokemonName;
            preStr = str.Substring(0, index);
            postStr = str.Substring(index);
            finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_pokemon_icon_c"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_pokemon_icon_c"].ToString(), false);
            sw.WriteLine(data["pFile_pokemon_icon_c"]);
            sw.Close();
        }

        private void graphics_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_graphics_h"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "extern const u8 gMonPic_Egg[];";
            int index = str.LastIndexOf(toSearch) - 1;
            string newString = "\nextern const u8 gMonFrontPic_" + firstMayusPokemonName + "[];" +
                "\nextern const u8 gMonPalette_" + firstMayusPokemonName + "[];" +
                "\nextern const u8 gMonBackPic_" + firstMayusPokemonName + "[];" +
                "\nextern const u8 gMonShinyPalette_" + firstMayusPokemonName + "[];" +
                "\nextern const u8 gMonIcon_" + firstMayusPokemonName + "[];" +
                "\nextern const u8 gMonFootprint_" + firstMayusPokemonName + "[];";
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_graphics_h"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_graphics_h"].ToString(), false);
            sw.WriteLine(data["pFile_graphics_h"]);
            sw.Close();
        }

        private void back_pic_table_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_back_pic_table_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "\tobj_tiles gMonPic_Egg";
            int index = str.LastIndexOf(toSearch) - 1;
            var newString = "\n\tobj_tiles gMonBackPic_" + firstMayusPokemonName + ", 0x800, SPECIES_" + mayusPokemonName;
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_back_pic_table_inc"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_back_pic_table_inc"].ToString(), false);
            sw.WriteLine(data["pFile_back_pic_table_inc"]);
            sw.Close();
        }

        private void front_pic_table_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_front_pic_table_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "\tobj_tiles gMonPic_Egg";
            int index = str.LastIndexOf(toSearch) - 1;
            var newString = "\n\tobj_tiles gMonFrontPic_" + firstMayusPokemonName + ", 0x800, SPECIES_" + mayusPokemonName;
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_front_pic_table_inc"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_front_pic_table_inc"].ToString(), false);
            sw.WriteLine(data["pFile_front_pic_table_inc"]);
            sw.Close();
        }

        private void graphics_inc_set()
        {
            //pFile_graphics_h
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_graphics_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "\t.align 2\ngMonPic_Egg";
            int index = str.LastIndexOf(toSearch) - 1;
            string newString = "\n\t.align 2\ngMonFrontPic_" + firstMayusPokemonName + "::" +
                "\n\t.incbin \"graphics/pokemon/" + minusPokemonName + "/front.4bpp.lz\"\n" +
                "\n\t.align 2" +
                "\ngMonPalette_" + firstMayusPokemonName + "::" +
                "\n\t.incbin \"graphics/pokemon/" + minusPokemonName + "/normal.gbapal.lz\"\n" +
                "\n\t.align 2" +
                "\ngMonBackPic_" + firstMayusPokemonName + "::" +
                "\n\t.incbin \"graphics/pokemon/" + minusPokemonName + "/back.4bpp.lz\"\n" +
                "\n\t.align 2" +
                "\ngMonShinyPalette_" + firstMayusPokemonName + "::" +
                "\n\t.incbin \"graphics/pokemon/" + minusPokemonName + "/shiny.gbapal.lz\"\n" +
                "\n\t.align 2" +
                "\ngMonIcon_" + firstMayusPokemonName + "::" +
                "\n\t.incbin \"graphics/pokemon/" + minusPokemonName + "/icon.4bpp\"\n" +
                "\n\t.align 2" +
                "\ngMonFootprint_" + firstMayusPokemonName + "::" +
                "\n\t.incbin \"graphics/pokemon/" + minusPokemonName + "/footprint.1bpp\"\n";
            string preStr = str.Substring(0, index);
            string postStr = str.Substring(index);
            string finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_graphics_inc"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_graphics_inc"].ToString(), false);
            sw.WriteLine(data["pFile_graphics_inc"]);
            sw.Close();
        }

        private void palette_table_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_palette_table_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "\tobj_pal gMonPalette_Egg";
            int index = str.LastIndexOf(toSearch) - 1;
            var newString = "\n\tobj_pal gMonPalette_" + firstMayusPokemonName + ", SPECIES_" + mayusPokemonName;
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_palette_table_inc"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_palette_table_inc"].ToString(), false);
            sw.WriteLine(data["pFile_palette_table_inc"]);
            sw.Close();
        }

        private void shiny_palette_table_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_shiny_palette_table_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "\tobj_pal gMonPalette_Egg";
            int index = str.LastIndexOf(toSearch) - 1;
            var newString = "\n\tobj_pal gMonShinyPalette_" + firstMayusPokemonName + ", PAL_ID_SHINY + SPECIES_" + mayusPokemonName;
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_shiny_palette_table_inc"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_shiny_palette_table_inc"].ToString(), false);
            sw.WriteLine(data["pFile_shiny_palette_table_inc"]);
            sw.Close();
        }

        private void back_pick_coords_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_back_pic_coords_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var backBytes = Regex.Matches(str, ".byte").Cast<Match>().Count() - 28;
            var index = 0;
            for (int i = 0; i < backBytes; i++)
            {
                index = str.IndexOf(".byte", index + 1);
            }
            index = str.IndexOf("\n", index);
            var newString = "\n\t.byte   0,  0, 0, 0";
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_back_pic_coords_inc"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_back_pic_coords_inc"].ToString(), false);
            sw.WriteLine(data["pFile_back_pic_coords_inc"]);
            sw.Close();

        }

        private void front_pick_coords_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_front_pic_coords_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            var frontBytes = Regex.Matches(str, ".byte").Cast<Match>().Count() - 28;
            var index = 0;
            for (int i = 0; i < frontBytes; i++)
            {
                index = str.IndexOf(".byte", index + 1);
            }
            index = str.IndexOf("\n", index);
            var newString = "\n\t.byte   0,  0, 0, 0";
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_front_pic_coords_inc"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_front_pic_coords_inc"].ToString(), false);
            sw.WriteLine(data["pFile_front_pic_coords_inc"]);
            sw.Close();
        }

        private void battle_1_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_battle_1_c"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "const u8 gEnemyMonElevation[]";
            int index = str.LastIndexOf(toSearch) - 1;
            index = str.IndexOf("};", index) - 1;
            var newString = "\n    [SPECIES_" + mayusPokemonName + "] = 0,";
            var preStr = str.Substring(0, index);
            var postStr = str.Substring(index);
            var finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;
            data["pFile_battle_1_c"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_battle_1_c"].ToString(), false);
            sw.WriteLine(data["pFile_battle_1_c"]);
            sw.Close();
        }

        private void direct_sound_data_set()
        {
            string str = null;
            StreamReader sr = new StreamReader(dictionary["pFile_direct_sound_data_inc"].ToString());
            str = sr.ReadToEnd();
            sr.Close();

            //INDEX DE LA ÚLTIMA POSICIÓN
            string toSearch = "\t.align 2\nDirectSoundWaveData_Phoneme_Vowel7";
            int index = str.LastIndexOf(toSearch) - 1;
            string newString = "\n\t.align 2\nCry_" + firstMayusPokemonName + "::\n\t.incbin \"sound/direct_sound_samples/cries/cry_" + minusPokemonName + ".bin\"\n";
            string preStr = str.Substring(0, index);
            string postStr = str.Substring(index);
            string finalString = preStr + newString + postStr;
            //richTextBox1.Text = finalString;

            data["pFile_direct_sound_data_inc"] = finalString;

            //richTextBox1.Text = str;

            StreamWriter sw = new StreamWriter(dictionary["pFile_direct_sound_data_inc"].ToString(), false);
            sw.WriteLine(data["pFile_direct_sound_data_inc"]);
            sw.Close();
        }

        private void data_reset()
        {
            var newIndex = cmbInforma_Species.Items.Count;

            pokemonData["psBase"][newIndex.ToString()] = "0";
            pokemonData["ataqueBase"][newIndex.ToString()] = "0";
            pokemonData["defensaBase"][newIndex.ToString()] = "0";
            pokemonData["velocidadBase"][newIndex.ToString()] = "0";
            pokemonData["ataqueEspecialBase"][newIndex.ToString()] = "0";
            pokemonData["defensaEspecialBase"][newIndex.ToString()] = "0";
            pokemonData["tipoUno"][newIndex.ToString()] = "TYPE_NORMAL";
            pokemonData["tipoDos"][newIndex.ToString()] = "TYPE_NORMAL";
            pokemonData["ratioDeCaptura"][newIndex.ToString()] = "0";
            pokemonData["expBase"][newIndex.ToString()] = "0";
            pokemonData["evsPS"][newIndex.ToString()] = "0";
            pokemonData["evsAtaque"][newIndex.ToString()] = "0";
            pokemonData["evsDefensa"][newIndex.ToString()] = "0";
            pokemonData["evsVelocidad"][newIndex.ToString()] = "0";
            pokemonData["evsAtaqueEspecial"][newIndex.ToString()] = "0";
            pokemonData["evsDefensaEspecial"][newIndex.ToString()] = "0";
            pokemonData["objetoUno"][newIndex.ToString()] = "ITEM_NONE";
            pokemonData["objetoDos"][newIndex.ToString()] = "ITEM_NONE";
            pokemonData["ratioGenero"][newIndex.ToString()] = "50";
            pokemonData["tieneGenero"][newIndex.ToString()] = "true";
            pokemonData["ciclosHuevo"][newIndex.ToString()] = "0";
            pokemonData["amistadBase"][newIndex.ToString()] = "0";
            pokemonData["crecimiento"][newIndex.ToString()] = "GROWTH_MEDIUM_FAST";
            pokemonData["grupoHuevoUno"][newIndex.ToString()] = "EGG_GROUP_FIELD";
            pokemonData["grupoHuevoDos"][newIndex.ToString()] = "EGG_GROUP_FIELD";
            pokemonData["habilidadUno"][newIndex.ToString()] = "ABILITY_NONE";
            pokemonData["habilidadDos"][newIndex.ToString()] = "ABILITY_NONE";
            pokemonData["probabilidadHuidaSafari"][newIndex.ToString()] = "0";
            pokemonData["colorCuerpo"][newIndex.ToString()] = "BODY_COLOR_GRAY";
            pokemonData["pokemonName"][newIndex.ToString()] = mayusPokemonName;
            moveData[newIndex.ToString() + "_0"] = Tuple.Create("1", "1", "MOVE_TACKLE");
            pokemonData["pokedexPageOne"][newIndex.ToString()] = "";
            pokemonData["pokedexPageTwo"][newIndex.ToString()] = "";
            pokemonData["categoriaPokemon"][newIndex.ToString()] = "NULL";
            pokemonData["altura"][newIndex.ToString()] = "0";
            pokemonData["peso"][newIndex.ToString()] = "0";
            pokemonData["escalaPokemon"][newIndex.ToString()] = "0";
            pokemonData["offsetPokemon"][newIndex.ToString()] = "0";
            pokemonData["escalaEntrenador"][newIndex.ToString()] = "0";
            pokemonData["offsetEntrenador"][newIndex.ToString()] = "0";
            pokemonData["frontCord"][newIndex.ToString()] = "0";
            pokemonData["backCord"][newIndex.ToString()] = "0";
            pokemonData["elevate"][newIndex.ToString()] = "0";
            pokemonData["palUsed"][newIndex.ToString()] = "0";
            cmbInforma_Species.Items.Add(mayusPokemonName);
            cmbPokedex_Species.Items.Add(mayusPokemonName);
            cmbGraphic_Species.Items.Add(mayusPokemonName);
        }

        private string resetAfterValues(int actualValue)
        {
            int counter = actualValue + 1;
            string newValues = "\n#define SPECIES_EGG " + counter;
            counter++;
            newValues += "\n\n#define SPECIES_UNOWN_B " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_C " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_D " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_E " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_F " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_G " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_H " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_I " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_J " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_K " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_L " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_M " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_N " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_O " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_P " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_Q " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_R " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_S " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_T " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_U " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_V " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_W " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_X " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_Y " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_Z " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_EMARK " + counter;
            counter++;
            newValues += "\n#define SPECIES_UNOWN_QMARK " + counter + "\n";
            return newValues;
        }

        private void LoadLanguage(string lang)
        {
            menuLanguageToolStripMenuItem.DropDownItems.Clear();
            foreach (KeyValuePair<string, Dictionary<string, string>> entry in Configuration.LanguageFiles)
            {
                ToolStripMenuItem langItem = new ToolStripMenuItem(entry.Key);
                langItem.Click += new EventHandler(menuLanguageToolStripMenuItem_Click);
                if (entry.Key == lang)
                    langItem.Checked = true;
                menuLanguageToolStripMenuItem.DropDownItems.Add(langItem);
            }

            menuFileToolStripMenuItem.Text = LoadLanguageText(lang, "menuFile");
            menuFolderToolStripMenuItem.Text = LoadLanguageText(lang, "menuFolder");
            menuSaveToolStripMenuItem.Text = LoadLanguageText(lang, "menuSave");
            menuSaveAllToolStripMenuItem.Text = LoadLanguageText(lang, "menuSaveAll");
            menuSavingToolStripMenuItem.Text = LoadLanguageText(lang, "menuSaving");
            menuSavingEmptyBaseStatsToolStripMenuItem.Text = LoadLanguageText(lang, "menuSavingEmptyBaseStats");
            menuOptionsToolStripMenuItem.Text = LoadLanguageText(lang, "menuOptions");
            menuLanguageToolStripMenuItem.Text = LoadLanguageText(lang, "menuLanguage");

            tabStats.Text = LoadLanguageText(lang, "tabStats");
            tabPokedex.Text = LoadLanguageText(lang, "tabPokedex");
            tabGraphics.Text = LoadLanguageText(lang, "tabGraphics");

            grpStatsStats.Text = LoadLanguageText(lang, "grpStatsStats");
            lblStats_BaseStats.Text = LoadLanguageText(lang, "lblStats_BaseStats");
            lblStats_EffordYield.Text = LoadLanguageText(lang, "lblStats_EffordYield");
            lblStatsName.Text = LoadLanguageText(lang, "lblStatsName");
            lblStatsHP.Text = LoadLanguageText(lang, "lblStatsHP") + ":";
            lblStatsAtk.Text = LoadLanguageText(lang, "lblStatsAtk") + ":";
            lblStatsDef.Text = LoadLanguageText(lang, "lblStatsDef") + ":";
            lblStatsSpAtk.Text = LoadLanguageText(lang, "lblStatsSpAtk") + ":";
            lblStatsSpDef.Text = LoadLanguageText(lang, "lblStatsSpDef") + ":";
            lblStatsSpd.Text = LoadLanguageText(lang, "lblStatsSpd") + ":";

            lblStatsCatchRat.Text = LoadLanguageText(lang, "lblStatsCatchRate");
            lblStatsSafariFleeRate.Text = LoadLanguageText(lang, "lblStatsSafariFleeRate");
            lblStatsExpYield.Text = LoadLanguageText(lang, "lblStatsExpYield");
            lblStatsGrowthRate.Text = LoadLanguageText(lang, "lblStatsGrowthRate");
            lblStatsBodyColor.Text = LoadLanguageText(lang, "lblStatsBodyColor");
            chkStats_gender.Text = LoadLanguageText(lang, "lblStatsGenderRatio");
            lblStatsEggCycle.Text = LoadLanguageText(lang, "lblStatsEggCycle");
            lblStatsFriendship.Text = LoadLanguageText(lang, "lblStatsFriendship");

            btnStats_New.Text = LoadLanguageText(lang, "btnStats_New");
            btnStats_Save.Text = LoadLanguageText(lang, "btnStats_Save");
            btnStats_SaveAll.Text = LoadLanguageText(lang, "btnStats_SaveAll");

            grpStats_Other.Text = LoadLanguageText(lang, "grpStats_Other");
            lblStats_Type.Text = LoadLanguageText(lang, "lblStats_Type");
            lblStats_EggGroup.Text = LoadLanguageText(lang, "lblStats_EggGroup");
            lblStats_Abilities.Text = LoadLanguageText(lang, "lblStats_Abilities");
            lblStats_Items.Text = LoadLanguageText(lang, "lblStats_Items");
        }
        private string LoadLanguageText(string lang, string textId)
        {
            string str = textId;
            try
            {
                Dictionary<string, string> text = Configuration.LanguageFiles[lang];
                if (text != null)
                    str = text[textId];
            }
            catch
            {
                //MessageBox.Show("There was a problem loading the selected translation for the element \"" + textId + "\".");
            }
            return str;
        }

        private void menuLanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration.Config["language"] = (sender as ToolStripMenuItem).Text;
            Configuration.SaveConfig();
            LoadLanguage((sender as ToolStripMenuItem).Text);
        }

        private void btnStats_SaveAll_Click(object sender, EventArgs e)
        {
            saveAllData();
        }

        private void menuSaveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAllData();
        }

        private void menuSavingEmptyBaseStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration.Config["hideEmptyBaseStats"] = (!menuSavingEmptyBaseStatsToolStripMenuItem.Checked).ToString().ToLower();
            Configuration.SaveConfig();
            menuSavingEmptyBaseStatsToolStripMenuItem.Checked = !menuSavingEmptyBaseStatsToolStripMenuItem.Checked;
        }

        private void SorryNotImplemented()
        {
            MessageBox.Show("Not ready yet...", "Sorry");
        }
    }
}

//MUCHIO ESPANIOL

