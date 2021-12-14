using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RazorLight;
using SchematicsProject.component_list;
using SchematicsProject.IndexClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SchematicsProject
{

    public class Engine
    {
        private IDictionary<string, Object> Model = new ExpandoObject() as IDictionary<string, Object>;
        private IDictionary<string, Object> Prompts = new ExpandoObject() as IDictionary<string, Object>;
        private IDictionary<string, Object> Enums = new ExpandoObject() as IDictionary<string, Object>;
        private IDictionary<string, Object> Types = new ExpandoObject() as IDictionary<string, Object>;
        string CurrentPath = "";
        string CurrentDirectory = "";

        public IConfiguration Configuration { get; set; }

        public Engine(IConfiguration Configuration)
        {
            this.Configuration = Configuration;
            if (this.Configuration.GetSection("Env").Value == "Publish")
            {

                CurrentPath = System.AppDomain.CurrentDomain.BaseDirectory;
                CurrentDirectory = Directory.GetCurrentDirectory();
            }
            else if (this.Configuration.GetSection("Env").Value == "Debug")
            {
                var Environment = System.Environment.CurrentDirectory;
                CurrentPath = Directory.GetParent(Environment).Parent.FullName.Replace("\\bin", "");
                CurrentDirectory = CurrentPath;
            }
        }



        public async Task Input(string Input)
        {

            Input = Regex.Replace(Input, @"\s+", " ");   

            var Command = Input.Split(" ");
            if (Command.Length < 3)
            {
                throw new ArgumentException("Invalid input");
            }

            if (Command[0] != "-t")
                throw new ArgumentException("invalid input"); 

            string CollectionPath = Path.Combine(CurrentPath, "collection.json");


            JObject Data = JObject.Parse(File.ReadAllText(CollectionPath));
            var Schematics = Data["schematics"];
            var Component = Schematics[Command[1]];

            if (Component == null)
            {
                throw new Exception("Schematics doesn't exist");
            }
            var FactoryPath = Schematics[Command[1]]["factory"];
            var TemplatePath = FactoryPath  + "./Files";
            JArray Required = null;
            ArrayList Required2 = new ArrayList();

            CollectionPath = Path.Combine(CurrentPath, Component["schema"].ToString());
            JObject Schema = JObject.Parse(File.ReadAllText(CollectionPath)); //getting json for wanted object
            var Dictionary = Schema.ToObject<Dictionary<string, Object>>();
            Required = Dictionary["required"] as JArray;
            Required2 = Required.ToObject<ArrayList>();
            //var templates = dictionary["templates"];
            if (Dictionary.ContainsKey("properties"))
            {
                var Properties = Dictionary["properties"];
                foreach (JProperty Property in (JToken)Properties)
                {
                    var Field = Property.Path;
                    if (Property.Value["type"].ToString() == "boolean")
                    {
                        bool Def = false;
                        if (Property.Value["default"].ToString() == "True")
                            Def = true;
                        Model.Add(Field, Def);
                    }
                    else
                    {
                        Model.Add(Field, Property.Value["default"]); //getting default value for every key
                    }
                    Prompts.Add(Field, Property.Value["x-prompt"]);
                    Enums.Add(Field, Property.Value["enum"]);
                    Types.Add(Field, Property.Value["type"]);
                }
            }
            
            Model["name"] = Command[2];
            ArrayList InputtedValues = new ArrayList();
            InputtedValues.Add("name");
            if (Command.Length > 3)
            {
                for (var i = 3; i < Command.Length; i++)
                {
                    var Command2 = Command[i].Split(":");
                    var firstPart = Command2[0];
                    var secondPart = Command2[1];
                    Model[firstPart] = secondPart;
                    InputtedValues.Add(firstPart);

                }
            }



            foreach (var Question in Prompts)
            {
                if (InputtedValues.Contains(Question.Key))
                    continue;
                Model = promptAction(Question, Required2);
            }

            ClassHelper(Command[1]);

            var DirFile = CurrentPath + TemplatePath + "/";
            

            foreach (string FileName in GetFiles(DirFile))
            {
                var TemplateName = FileName.Replace(CurrentPath + TemplatePath + "/", "");
                TemplateName = TemplateName.Replace(@"\", "/");
                
                //templateName = templateName.Split('/').Last();
                TemplateName = TemplateName.Replace(CurrentPath + TemplatePath + "/", "");
                string Pattern = @"__(.*?)__";
                Regex Regex = new Regex(Pattern);
                foreach (Match match in Regex.Matches(TemplateName, Pattern))
                {
                    TemplateName = Regex.Replace(TemplateName, Model[match.Groups[1].Value].ToString(), 1);
                }

                int Index = TemplateName.LastIndexOf(".");
                if (Index >= 0)
                    TemplateName = TemplateName.Substring(0, Index);
                Generate(FileName, TemplateName);
            }
        }

        public IEnumerable<string> GetFiles(string Path)
        {
            var InitialPath = Path;
            Queue<string> Queue = new Queue<string>();
            Queue.Enqueue(Path);
            while (Queue.Count > 0)
            {
                Path = Queue.Dequeue();
                //Console.WriteLine("Path " + path);
                try
                {
                    foreach (string SubDir in Directory.GetDirectories(Path))
                    {
                        
                        var DirectoryInsideTemplates = SubDir.Replace(InitialPath, "");
                        var PlaceToCreateDirectory = CurrentDirectory + "/" + DirectoryInsideTemplates;
                        System.IO.Directory.CreateDirectory(PlaceToCreateDirectory);

                        Queue.Enqueue(SubDir + "/");
                    }
                }
                catch (Exception Ex)
                {
                    Console.Error.WriteLine(Ex);
                }
                string[] Files = null;
                try
                {
                    Files = Directory.GetFiles(Path);
                }
                catch (Exception Ex)
                {
                    Console.Error.WriteLine(Ex);
                }
                if (Files != null)
                {
                    for (int i = 0; i < Files.Length; i++)
                    {
                        yield return Files[i];
                    }
                }
            }
        }

        public void ClassHelper(string Command) //Ovaj dio ne znam je li potreban, ali svaka komponenta koja se moze generisati
                                                //bi trebalo da moze imati dodatne stvari koje su karakteristicne za nju
                                                //da se mogu dodati, npr. za component su to filteri
        {
            if (Command == "component")
            {
                Model = Component.AddDynamic(Model);
            }
            else if (Command == "component-list")
            {
                Model = ComponentList.AddDynamic(Model);
            }
        }

        public dynamic promptAction(dynamic Question, dynamic Required)
        {
            if (Question.Value != null)
            {
                Console.CursorVisible = true;
                Console.WriteLine(Question.Value);
                if (Enums[Question.Key] != null)
                {
                    Console.CursorVisible = false;
                    IEnumerable EnumerableQuestions = Enums[Question.Key] as IEnumerable;
                    List<string> Options = new List<string>();
                    foreach (var Option in EnumerableQuestions)
                    {
                        Options.Add(Option.ToString());
                    }


                    Menu Menu = new Menu(Options.ToArray());
                    int SelectedIndex = Menu.Run();
                    Model[Question.Key] = Options[SelectedIndex];

                }

                else if (Types[Question.Key].ToString() == "boolean")
                {
                    var Def = Console.ReadLine();
                    if (Def != "")
                    {
                        var res = Model[Question.Key];
                        if (Def == "No" || Def == "no" || Def == "n" || Def == "NO")
                            res = false;
                        else if (Def == "Yes" || Def == "yes" || Def == "y" || Def == "YES")
                            res = true;
                        Model[Question.Key] = res;
                    }
                }
                else
                {
                    var InputtedValue = Console.ReadLine();
                    if (Required.Contains(Question.Key) && InputtedValue == "")
                        Model[Question.Key] = Model["name"];
                    if (InputtedValue != "")
                        Model[Question.Key] = InputtedValue;
                }
                Console.WriteLine("");
            }
            return Model;
        }

        public async void Generate(string templatePath, string OutputFileName)
        {
            string DestinationPath = CurrentDirectory + "/";
            var Engine = new RazorLightEngineBuilder()
                  // required to have a default RazorLightProject type, but not required to create a template from string.
                  .UseEmbeddedResourcesProject(typeof(ComponentList))
                  .UseMemoryCachingProvider()
                  .Build();

            string Template = "";
            try
            {
                Template = File.ReadAllText(templatePath);
            }
            catch (Exception e)
            {
                throw e;
            }

            string Result = await Engine.CompileRenderStringAsync("templateKey2", Template, Model);
            

            //string outputName = model["name"].ToString() + "." + language;

            

            using (StreamWriter OutputFile = new StreamWriter(System.IO.Path.Combine(DestinationPath, OutputFileName)))
            {
                await OutputFile.WriteAsync(Result);
                Console.WriteLine("Successfully generated " + OutputFileName);
            }
        }
    }
}







