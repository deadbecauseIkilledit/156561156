using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CurriculumSystem;  // Use the namespace we created
using System.Threading.Tasks;

public class SimpleCurriculumParser : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private bool useFirestore = true;
    [SerializeField] private bool useJsonFile = false;
    [SerializeField] private TextAsset curriculumJsonFile;
    
    [Header("Loading State")]
    [SerializeField] private bool isLoading = false;
    [SerializeField] private string loadingStatus = "Not started";
    
    private FirestoreDataService firestoreService;
    
    // Public properties to access parsed data
    public Dictionary<string, Exercise> AllExercises { get; private set; }
    public Dictionary<string, string> GlossaryMap { get; private set; }
    public List<ExerciseProgressionData> Progressions { get; private set; }
    
    // Store full curriculum data for hierarchical tree building
    private CurriculumExportData fullData;
    public CurriculumExportData FullCurriculumData => fullData;
    
    // Events for data loading
    public event Action<bool> OnDataLoaded;
    public event Action<string> OnLoadingStatusChanged;
    
    void Awake()
    {
        InitializeCollections();
        
        if (useFirestore)
        {
            Debug.Log("Will load from Firestore...");
            InitializeFirestoreConnection();
        }
        else if (useJsonFile && curriculumJsonFile != null)
        {
            Debug.Log("Attempting to load from JSON file...");
            TryParseJson(curriculumJsonFile.text);
        }
        else
        {
            Debug.Log("Loading sample data...");
            LoadSampleData();
        }
    }
    
    void InitializeCollections()
    {
        AllExercises = new Dictionary<string, Exercise>();
        GlossaryMap = new Dictionary<string, string>();
        Progressions = new List<ExerciseProgressionData>();
    }
    
    void TryParseJson(string jsonText)
    {
        try
        {
            // Try to parse with JsonUtility
            var data = JsonUtility.FromJson<CurriculumExportData>(jsonText);
            
            if (data != null && data.collections != null)
            {
                ProcessParsedData(data);
            }
            else
            {
                Debug.LogWarning("JSON parsing returned null, using sample data");
                LoadSampleData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON parsing failed: {e.Message}");
            Debug.Log("Falling back to sample data");
            LoadSampleData();
        }
    }
    
    void ProcessParsedData(CurriculumExportData data)
    {
        // Store the full data
        fullData = data;
        
        // Process glossary
        if (data.collections.glossary != null)
        {
            foreach (var item in data.collections.glossary)
            {
                if (!string.IsNullOrEmpty(item.key))
                {
                    GlossaryMap[item.key] = item.description;
                }
            }
        }
        
        // Process exercises from disciplines
        if (data.collections.disciplines_v3 != null)
        {
            foreach (var discipline in data.collections.disciplines_v3)
            {
                if (discipline.curriculums == null) continue;
                
                foreach (var curriculum in discipline.curriculums)
                {
                    if (curriculum.groups == null) continue;
                    
                    foreach (var group in curriculum.groups)
                    {
                        // Get exercises from group
                        if (group.exercises != null)
                        {
                            foreach (var exercise in group.exercises)
                            {
                                AllExercises[exercise.id] = exercise;
                            }
                        }
                        
                        // Get exercises from modules
                        if (group.modules != null)
                        {
                            foreach (var module in group.modules)
                            {
                                if (module.exercises != null)
                                {
                                    foreach (var exercise in module.exercises)
                                    {
                                        AllExercises[exercise.id] = exercise;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        // Process progressions
        if (data.collections.exercise_progressions != null)
        {
            Progressions = data.collections.exercise_progressions;
        }
        
        Debug.Log($"Loaded from JSON: {AllExercises.Count} exercises, {Progressions.Count} progressions");
    }
    
    public void LoadSampleData()
    {
        // Create full sample curriculum structure
        fullData = new CurriculumExportData
        {
            exportDate = DateTime.Now.ToString(),
            version = "1.0",
            collections = new Collections
            {
                disciplines_v3 = new List<Discipline>(),
                glossary = new List<GlossaryItem>(),
                exercise_progressions = new List<ExerciseProgressionData>()
            }
        };
        
        // Create sample discipline
        var calisthenicsDiscipline = new Discipline
        {
            id = "discipline_001",
            name = "Calisthenics",
            description_key = "calisthenics_desc",
            is_active = true,
            curriculums = new List<Curriculum>()
        };
        
        // Create sample curriculum
        var beginnerCurriculum = new Curriculum
        {
            id = "curriculum_001",
            name = "Beginner Program",
            description_key = "beginner_program_desc",
            is_active = true,
            groups = new List<Group>()
        };
        
        // Create sample groups
        var pushGroup = new Group
        {
            id = "group_push",
            name = "Push Exercises",
            description_key = "push_exercises_desc",
            is_active = true,
            exercises = new List<Exercise>(),
            modules = new List<Module>()
        };
        
        var pullGroup = new Group
        {
            id = "group_pull",
            name = "Pull Exercises",
            description_key = "pull_exercises_desc",
            is_active = true,
            exercises = new List<Exercise>(),
            modules = new List<Module>()
        };
        
        // Create sample exercises
        var pushups = new Exercise
        {
            id = "ex_001",
            name = "Push-ups",
            description_key = "basic_pushups",
            position = 1,
            is_active = true,
            tags = new List<string> { "beginner", "chest" },
            equipment_required = new List<string>(),
            benchmarks = new List<string> { "10 reps", "20 reps", "30 reps" }
        };
        
        var diamondPushups = new Exercise
        {
            id = "ex_002",
            name = "Diamond Push-ups",
            description_key = "diamond_pushups",
            position = 2,
            is_active = true,
            tags = new List<string> { "intermediate", "chest", "triceps" },
            equipment_required = new List<string>(),
            benchmarks = new List<string> { "5 reps", "10 reps", "15 reps" }
        };
        
        var archerPushups = new Exercise
        {
            id = "ex_003",
            name = "Archer Push-ups",
            description_key = "archer_pushups",
            position = 3,
            is_active = true,
            tags = new List<string> { "advanced", "chest" },
            equipment_required = new List<string>(),
            benchmarks = new List<string> { "3 reps each side", "5 reps each side" }
        };
        
        var pullups = new Exercise
        {
            id = "ex_004",
            name = "Pull-ups",
            description_key = "basic_pullups",
            position = 1,
            is_active = true,
            tags = new List<string> { "intermediate", "back" },
            equipment_required = new List<string> { "Pull-up bar" },
            benchmarks = new List<string> { "5 reps", "10 reps", "15 reps" }
        };
        
        var muscleups = new Exercise
        {
            id = "ex_005",
            name = "Muscle-ups",
            description_key = "muscle_ups",
            position = 2,
            is_active = true,
            tags = new List<string> { "advanced", "back", "chest" },
            equipment_required = new List<string> { "Pull-up bar" },
            benchmarks = new List<string> { "1 rep", "3 reps", "5 reps" }
        };
        
        // Add exercises to groups
        pushGroup.exercises.Add(pushups);
        pushGroup.exercises.Add(diamondPushups);
        pushGroup.exercises.Add(archerPushups);
        
        pullGroup.exercises.Add(pullups);
        pullGroup.exercises.Add(muscleups);
        
        // Build the hierarchy
        beginnerCurriculum.groups.Add(pushGroup);
        beginnerCurriculum.groups.Add(pullGroup);
        
        calisthenicsDiscipline.curriculums.Add(beginnerCurriculum);
        
        fullData.collections.disciplines_v3.Add(calisthenicsDiscipline);
        
        // Add to dictionary
        AllExercises[pushups.id] = pushups;
        AllExercises[diamondPushups.id] = diamondPushups;
        AllExercises[archerPushups.id] = archerPushups;
        AllExercises[pullups.id] = pullups;
        AllExercises[muscleups.id] = muscleups;
        
        // Create sample glossary
        GlossaryMap["basic_pushups"] = "A fundamental upper body exercise targeting chest, shoulders, and triceps";
        GlossaryMap["diamond_pushups"] = "Push-ups with hands forming a diamond shape, increasing tricep activation";
        GlossaryMap["archer_pushups"] = "Advanced push-up variation working one arm at a time";
        GlossaryMap["basic_pullups"] = "Fundamental pulling exercise for back and biceps";
        GlossaryMap["muscle_ups"] = "Advanced movement combining a pull-up and dip";
        
        // Create progression chains
        var pushupProgression = new ExerciseProgressionData
        {
            id = "prog_pushups",
            curriculum_id = "curriculum_001",
            parent_id = "group_push",
            parent_type = "group",
            chains = new List<Chain>
            {
                new Chain
                {
                    id = "chain_001",
                    color = "#3B82F6",  // Blue
                    exercise_ids = new List<string> { pushups.id, diamondPushups.id, archerPushups.id }
                }
            }
        };
        
        var pullupProgression = new ExerciseProgressionData
        {
            id = "prog_pullups",
            curriculum_id = "curriculum_001",
            parent_id = "group_pull",
            parent_type = "group",
            chains = new List<Chain>
            {
                new Chain
                {
                    id = "chain_002",
                    color = "#10B981",  // Green
                    exercise_ids = new List<string> { pullups.id, muscleups.id }
                }
            }
        };
        
        Progressions.Add(pushupProgression);
        Progressions.Add(pullupProgression);
        
        // Also add to fullData
        fullData.collections.exercise_progressions.Add(pushupProgression);
        fullData.collections.exercise_progressions.Add(pullupProgression);
        
        Debug.Log($"Sample data loaded: {AllExercises.Count} exercises, {Progressions.Count} progressions, {fullData.collections.disciplines_v3.Count} disciplines");
    }
    
    // Helper methods
    void InitializeFirestoreConnection()
    {
        firestoreService = FirestoreDataService.Instance;
        
        if (firestoreService.IsInitialized)
        {
            LoadDataFromFirestore();
        }
        else
        {
            firestoreService.OnInitialized += OnFirestoreInitialized;
        }
    }
    
    void OnFirestoreInitialized(bool success)
    {
        if (success)
        {
            LoadDataFromFirestore();
        }
        else
        {
            Debug.LogError("Failed to initialize Firestore, falling back to sample data");
            LoadSampleData();
            OnDataLoaded?.Invoke(false);
        }
    }
    
    async void LoadDataFromFirestore()
    {
        isLoading = true;
        UpdateLoadingStatus("Connecting to Firestore...");
        
        try
        {
            // Load glossary first
            UpdateLoadingStatus("Loading glossary...");
            GlossaryMap = await firestoreService.FetchGlossaryAsync();
            Debug.Log($"Loaded {GlossaryMap.Count} glossary items");
            
            // Load disciplines with all nested data
            UpdateLoadingStatus("Loading disciplines and exercises...");
            var disciplines = await firestoreService.FetchDisciplinesAsync();
            
            // Extract all exercises from the nested structure
            AllExercises.Clear();
            foreach (var discipline in disciplines)
            {
                if (discipline.curriculums == null) continue;
                
                foreach (var curriculum in discipline.curriculums)
                {
                    if (curriculum.groups == null) continue;
                    
                    foreach (var group in curriculum.groups)
                    {
                        // Add exercises from group
                        if (group.exercises != null)
                        {
                            foreach (var exercise in group.exercises)
                            {
                                AllExercises[exercise.id] = exercise;
                            }
                        }
                        
                        // Add exercises from modules
                        if (group.modules != null)
                        {
                            foreach (var module in group.modules)
                            {
                                if (module.exercises != null)
                                {
                                    foreach (var exercise in module.exercises)
                                    {
                                        AllExercises[exercise.id] = exercise;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            Debug.Log($"Loaded {AllExercises.Count} exercises from Firestore");
            
            // Load progressions
            UpdateLoadingStatus("Loading exercise progressions...");
            Progressions = await firestoreService.FetchProgressionsAsync();
            Debug.Log($"Loaded {Progressions.Count} progressions");
            
            // Subscribe to real-time updates
            if (disciplines.Count > 0 && disciplines[0].curriculums != null && disciplines[0].curriculums.Count > 0)
            {
                var firstCurriculum = disciplines[0].curriculums[0];
                firestoreService.SubscribeToProgressionUpdates(firstCurriculum.id);
                firestoreService.OnProgressionsUpdated += OnProgressionsUpdated;
            }
            
            firestoreService.SubscribeToGlossaryUpdates();
            firestoreService.OnGlossaryUpdated += OnGlossaryUpdated;
            
            isLoading = false;
            UpdateLoadingStatus("Data loaded successfully");
            OnDataLoaded?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading from Firestore: {e.Message}");
            UpdateLoadingStatus($"Error: {e.Message}");
            isLoading = false;
            
            // Fall back to sample data
            LoadSampleData();
            OnDataLoaded?.Invoke(false);
        }
    }
    
    void OnProgressionsUpdated(List<ExerciseProgressionData> progressions)
    {
        Progressions = progressions;
        Debug.Log($"Progressions updated in real-time: {progressions.Count} items");
    }
    
    void OnGlossaryUpdated(Dictionary<string, string> glossary)
    {
        GlossaryMap = glossary;
        Debug.Log($"Glossary updated in real-time: {glossary.Count} items");
    }
    
    void UpdateLoadingStatus(string status)
    {
        loadingStatus = status;
        OnLoadingStatusChanged?.Invoke(status);
        Debug.Log($"Loading status: {status}");
    }
    
    public string GetDescription(string descriptionKey)
    {
        return GlossaryMap.ContainsKey(descriptionKey) ? GlossaryMap[descriptionKey] : descriptionKey;
    }
    
    public Exercise GetExercise(string exerciseId)
    {
        return AllExercises.ContainsKey(exerciseId) ? AllExercises[exerciseId] : null;
    }


     public Dictionary<string, Exercise> GetAllExercises()
    {
        return AllExercises;
    }
    
    public List<ExerciseProgressionData> GetProgressions()
    {
        return Progressions;
    }
    
    public bool IsLoading => isLoading;
    public string LoadingStatus => loadingStatus;
    
    void OnDestroy()
    {
        if (firestoreService != null)
        {
            firestoreService.OnProgressionsUpdated -= OnProgressionsUpdated;
            firestoreService.OnGlossaryUpdated -= OnGlossaryUpdated;
        }
    }
}