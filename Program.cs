using RszTool;

string dir = "../../../lib/stages";
string[] files = Directory.GetFiles(dir);

foreach (string file in files)
{
    // Read each stage file
    RszFileOption option = new(GameName.mhwilds);
    UserFile userFile = new(option, new FileHandler(file));
    userFile.Read();

    // Get the stage file name from path
    String stage = file.Substring(20);

    // Create the new, edited, stage file
    using FileHandler newFileHandler = new("../../../output/" + stage, true);

    RszInstance i = userFile.RSZ.ObjectList[0];

    // Flags to set tempered/more spawn variants
    Boolean tempered = false;
    Boolean manyMonsters = false;

    // Set Max Strength for each category of monster population
    SetMaxStrength("_NormalPopParamsByHR", "_NormalPopParams", i, tempered);
    SetMaxStrengthSwarm("_SwarmPopParamsByHR", "_SwarmPopParams", i, tempered);
    SetMaxStrength("_PopManyPopParamsByHR", "_PopManyPopParams", i, tempered);
    SetMaxStrength("_PopManyPopParamsByHR_2", "_PopManyPopParams_2", i, tempered);
    SetMaxStrength("_CocoonPopParamsByHR", "_CocoonPopParams", i, tempered);
    SetMaxStrength("_FrenzyPopParamsByHR", "_FrenzyPopParams", i, tempered);
    SetMaxStrength("_NushiPopParamsByHR", "_NushiPopParams", i, tempered);
    SetMaxStrength("_BattlefieldPopParamsByHR", "_BattlefieldPopParams", i, tempered);
    SetMaxStrength("_LegendaryPopParamsByHR", "_LegendaryPopParams", i, tempered);

    if (manyMonsters)
    {
        SetManyMonsters(i);
    }

    userFile.WriteTo(newFileHandler);
}

static void SetManyMonsters(RszInstance i)
{
    RszInstance popLimit = (RszInstance)i.GetFieldValue("_PopLimitByEnv");
    List<Object> envParams = (List<Object>)popLimit.GetFieldValue("_EnvParams");
    RszInstance hrParams = (RszInstance)envParams[envParams.Count - 1];

    Byte limit = (Byte)hrParams.GetFieldValue("_SpeciesLimit");
    Byte operand = 10;
    Byte res = (Byte)(limit * operand);
    hrParams.SetFieldValue("_SpeciesLimit", res);

    RszTool.via.Range range = (RszTool.via.Range)hrParams.GetFieldValue("_PopLimitRange");
    range.s = range.s * 8;
    range.r = range.r * 8;
    hrParams.SetFieldValue("_PopLimitRange", range);

    RszTool.via.Range rangeMany = (RszTool.via.Range)hrParams.GetFieldValue("_PopLimitRangeInMany");
    rangeMany.s = rangeMany.s * 2;
    rangeMany.r = rangeMany.r * 2;
    hrParams.SetFieldValue("_PopLimitRangeInMany", rangeMany);
}

static void SetMaxStrength(String hrObjName, String popObjName, RszInstance i, Boolean tempered)
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
        if (tempered)
        {
            enemyInstance.SetFieldValue("_LegendaryProbability", (Byte)100);
        }
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
        foreach (var rank in difficultyRanks)
        {
            RszInstance rankInst = (RszInstance)rank;
            rankInst.SetFieldValue("_RandomWeight", (Byte)0);
        }
        // set max valid difficulty prob to 100
        RszInstance maxDifficultyInst = (RszInstance)difficultyRanks.ElementAt(max);
        maxDifficultyInst.SetFieldValue("_RandomWeight", (Byte)100);
    }
}

static void SetMaxStrengthSwarm(String hrObjName, String popObjName, RszInstance i, Boolean tempered)
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

        if (tempered)
        {
            enemyInstance.SetFieldValue("_LegendaryProbability", (Byte)100);
            enemyInstance.SetFieldValue("_BossLegendaryProbability", (Byte)100);
        }


        List<string> strList = new List<string>();
        strList.Add("_DifficultyParams");
        strList.Add("_BossDifficultyParams");
        foreach (String s in strList)
        {
            List<Object> difficultyRanks = (List<Object>)enemyInstance.GetFieldValue(s);

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
            foreach (var rank in difficultyRanks)
            {
                RszInstance rankInst = (RszInstance)rank;
                rankInst.SetFieldValue("_RandomWeight", (Byte)0);
            }
            // set max valid difficulty prob to 100
            RszInstance maxDifficultyInst = (RszInstance)difficultyRanks.ElementAt(max);
            maxDifficultyInst.SetFieldValue("_RandomWeight", (Byte)100);
        }
    }
}
