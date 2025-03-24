using RszTool;

string dir= "test/stages/";
string[] files = Directory.GetFiles(dir);

foreach (string file in files)
{
    // Read each stage file
    RszFileOption option = new(GameName.mhrise);
    UserFile userFile = new(option, new FileHandler(file));
    userFile.Read();
    
    // Get the stage file name from path
    String stage = file.Substring(12, 14);

    // Create the new, edited, stage file
    using FileHandler newFileHandler = new("test/complete/"+stage+"_new.3", true);
    userFile.WriteTo(newFileHandler);

    RszInstance i = userFile.RSZ.ObjectList[0];
    
    // Set Max Strength for each category of monster population
    SetMaxStrength("_NormalPopParamsByHR", "_NormalPopParams", i);
    SetMaxStrength("_SwarmPopParamsByHR", "_SwarmPopParams", i);
    SetMaxStrength("_PopManyPopParamsByHR", "_PopManyPopParams", i);
    SetMaxStrength("_PopManyPopParamsByHR_2", "_PopManyPopParams_2", i);
    SetMaxStrength("_LegendaryPopParamsByHR", "_LegendaryPopParams", i);
    SetMaxStrength("_CoccoonPopParamsByHR", "_CoccoonPopParams", i);
    SetMaxStrength("_FrenzyPopParamsByHR", "_FrenzyPopParams", i);
    SetMaxStrength("_NushiPopParamsByHR", "_NushiPopParams", i);
    SetMaxStrength("_BattlefieldPopParamsByHR", "_BattlefieldPopParams", i);

    userFile.Write();
}

static void SetMaxStrength(String hrObjName, String popObjName, RszInstance i)
{
    // Get the list of params - may be null for certain categories
    List<Object> popParams = (List<Object>)i.GetFieldValue(hrObjName);
    if (popParams == null)
    {
        return;
    }

    // Get the highest HR preset for the spawn table
    RszInstance hrParam = (RszInstance)popParams[popParams.Count - 1];
    List<Object> enemyParams = (List<Object>)hrParam.GetFieldValue(popObjName);
    // Update spawn table for each enemy in the preset
    foreach (var enemy in enemyParams)
    {
        RszInstance enemyInstance = (RszInstance)enemy;
        List<Object> difficultyRanks = (List<Object>)enemyInstance.GetFieldValue("_DifficultyParams");

        // Difficuly ordering is ascending, last object is the highest difficulty rank (as of now)
        int max = difficultyRanks.Count - 1;
        RszInstance maxDifficulty = (RszInstance)difficultyRanks[max];

        // Some monster difficulty presets are not "suitable" - it seems there is no reward table data for them
        // So, set the probability of the most difficult "suitable" spawn to 100
        Boolean suitable = (Boolean)maxDifficulty.GetFieldValue("_Suitable");
        while (!suitable)
        {
            max--;
            if (max < 0)
            {
                break;
            }
            maxDifficulty = (RszInstance)difficultyRanks[max];
            suitable = (Boolean)maxDifficulty.GetFieldValue("_Suitable");
        }
        if (!suitable)
        {
            // Just in case
            Console.WriteLine("doesn't spawn..");
            continue;
        }
        // Set all probabilities to 0
        foreach(var rank in difficultyRanks)
        {
            RszInstance rankInst = (RszInstance)rank;
            rankInst.SetFieldValue("_RandomWeight", (Byte)0);
        }
        // set max valid difficulty prob to 100
        RszInstance maxDifficultyInst = (RszInstance)difficultyRanks.ElementAt(max);
        maxDifficultyInst.SetFieldValue("_RandomWeight", (Byte)100);
    }
}