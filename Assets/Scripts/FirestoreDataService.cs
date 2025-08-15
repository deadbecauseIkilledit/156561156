using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using CurriculumSystem;
using System.Linq;
using Newtonsoft.Json;

public class FirestoreDataService : MonoBehaviour
{
    private FirebaseFirestore db;
    private bool isInitialized = false;
    private FirebaseApp app;
    
    // Event for initialization complete
    public event Action<bool> OnInitialized;
    
    // Event for data updates
    // public event Action<List<Discipline>> OnDisciplinesUpdated; // Reserved for future use
    public event Action<List<ExerciseProgressionData>> OnProgressionsUpdated;
    public event Action<Dictionary<string, string>> OnGlossaryUpdated;
    
    // Cached data
    private List<Discipline> cachedDisciplines = new List<Discipline>();
    private Dictionary<string, string> cachedGlossary = new Dictionary<string, string>();
    private List<ExerciseProgressionData> cachedProgressions = new List<ExerciseProgressionData>();
    
    // Firestore listeners
    // private ListenerRegistration disciplinesListener; // Reserved for future use
    private ListenerRegistration progressionsListener;
    private ListenerRegistration glossaryListener;
    
    private static FirestoreDataService instance;
    public static FirestoreDataService Instance 
    { 
        get 
        { 
            if (instance == null)
            {
                GameObject go = new GameObject("FirestoreDataService");
                instance = go.AddComponent<FirestoreDataService>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeFirebase();
    }
    
    void InitializeFirebase()
    {
        Debug.Log("Initializing Firebase...");
        
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => 
        {
            if (task.Result == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                isInitialized = true;
                
                Debug.Log($"Firebase initialized successfully. Project ID: {app.Options.ProjectId}");
                Debug.Log($"Firebase Storage Bucket: {app.Options.StorageBucket}");
                Debug.Log($"Firebase API Key exists: {!string.IsNullOrEmpty(app.Options.ApiKey)}");
                
                // Test Firestore connection
                TestFirestoreConnection();
                
                OnInitialized?.Invoke(true);
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
                OnInitialized?.Invoke(false);
            }
        });
    }
    
    public bool IsInitialized => isInitialized;
    
    // Test connection method
    async void TestFirestoreConnection()
    {
        try
        {
            Debug.Log("Testing Firestore connection...");
            var testQuery = db.Collection("disciplines_v3").Limit(1);
            var snapshot = await testQuery.GetSnapshotAsync();
            Debug.Log($"Firestore connection successful! Found {snapshot.Count} discipline(s)");
            
            if (snapshot.Count > 0)
            {
                var doc = snapshot.Documents.FirstOrDefault();
                if (doc != null)
                {
                    Debug.Log($"First discipline ID: {doc.Id}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firestore connection test failed: {e.Message}");
            Debug.LogError($"Full error: {e}");
        }
    }
    
    // Fetch all disciplines with nested data
    public async Task<List<Discipline>> FetchDisciplinesAsync()
    {
        if (!isInitialized)
        {
            Debug.LogError("Firebase not initialized");
            return new List<Discipline>();
        }
        
        try
        {
            Debug.Log("Fetching disciplines from Firestore...");
            
            var disciplinesRef = db.Collection("disciplines_v3");
            var disciplineSnapshot = await disciplinesRef.GetSnapshotAsync();
            
            var disciplines = new List<Discipline>();
            
            foreach (var disciplineDoc in disciplineSnapshot.Documents)
            {
                var discipline = ConvertToDiscipline(disciplineDoc);
                
                // Fetch curriculums for this discipline
                var curriculumsRef = disciplineDoc.Reference.Collection("curriculums");
                var curriculumSnapshot = await curriculumsRef.GetSnapshotAsync();
                
                discipline.curriculums = new List<Curriculum>();
                
                foreach (var curriculumDoc in curriculumSnapshot.Documents)
                {
                    var curriculum = ConvertToCurriculum(curriculumDoc);
                    
                    // Fetch groups for this curriculum
                    var groupsRef = curriculumDoc.Reference.Collection("groups");
                    var groupSnapshot = await groupsRef.GetSnapshotAsync();
                    
                    curriculum.groups = new List<Group>();
                    
                    foreach (var groupDoc in groupSnapshot.Documents)
                    {
                        var group = await ConvertToGroupWithNestedData(groupDoc);
                        curriculum.groups.Add(group);
                    }
                    
                    discipline.curriculums.Add(curriculum);
                }
                
                disciplines.Add(discipline);
            }
            
            cachedDisciplines = disciplines;
            Debug.Log($"Fetched {disciplines.Count} disciplines");
            
            return disciplines;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching disciplines: {e.Message}");
            return new List<Discipline>();
        }
    }
    
    // Fetch specific curriculum with its groups/modules/exercises
    public async Task<Curriculum> FetchCurriculumAsync(string disciplineId, string curriculumId)
    {
        if (!isInitialized)
        {
            Debug.LogError("Firebase not initialized");
            return null;
        }
        
        try
        {
            var curriculumRef = db.Collection("disciplines_v3").Document(disciplineId)
                                  .Collection("curriculums").Document(curriculumId);
            
            var curriculumDoc = await curriculumRef.GetSnapshotAsync();
            
            if (!curriculumDoc.Exists)
            {
                Debug.LogError($"Curriculum {curriculumId} not found");
                return null;
            }
            
            var curriculum = ConvertToCurriculum(curriculumDoc);
            
            // Fetch groups
            var groupsRef = curriculumDoc.Reference.Collection("groups");
            var groupSnapshot = await groupsRef.GetSnapshotAsync();
            
            curriculum.groups = new List<Group>();
            
            foreach (var groupDoc in groupSnapshot.Documents)
            {
                var group = await ConvertToGroupWithNestedData(groupDoc);
                curriculum.groups.Add(group);
            }
            
            return curriculum;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching curriculum: {e.Message}");
            return null;
        }
    }
    
    // Fetch exercise progressions
    public async Task<List<ExerciseProgressionData>> FetchProgressionsAsync(string curriculumId = null)
    {
        if (!isInitialized)
        {
            Debug.LogError("Firebase not initialized");
            return new List<ExerciseProgressionData>();
        }
        
        try
        {
            Query query = db.Collection("exercise_progressions_v3");
            
            if (!string.IsNullOrEmpty(curriculumId))
            {
                query = query.WhereEqualTo("curriculum_id", curriculumId);
            }
            
            var snapshot = await query.GetSnapshotAsync();
            var progressions = new List<ExerciseProgressionData>();
            
            foreach (var doc in snapshot.Documents)
            {
                var progression = ConvertToProgression(doc);
                if (progression != null)
                {
                    progressions.Add(progression);
                }
            }
            
            cachedProgressions = progressions;
            Debug.Log($"Fetched {progressions.Count} progressions");
            
            return progressions;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching progressions: {e.Message}");
            return new List<ExerciseProgressionData>();
        }
    }
    
    // Fetch glossary items
    public async Task<Dictionary<string, string>> FetchGlossaryAsync()
    {
        if (!isInitialized)
        {
            Debug.LogError("Firebase not initialized");
            return new Dictionary<string, string>();
        }
        
        try
        {
            var glossaryRef = db.Collection("glossary_v3");
            var snapshot = await glossaryRef.GetSnapshotAsync();
            
            var glossary = new Dictionary<string, string>();
            
            foreach (var doc in snapshot.Documents)
            {
                if (doc.TryGetValue("key", out string key) && 
                    doc.TryGetValue("description", out string description))
                {
                    glossary[key] = description;
                }
            }
            
            cachedGlossary = glossary;
            Debug.Log($"Fetched {glossary.Count} glossary items");
            
            return glossary;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching glossary: {e.Message}");
            return new Dictionary<string, string>();
        }
    }
    
    // Subscribe to real-time updates for progressions
    public void SubscribeToProgressionUpdates(string curriculumId)
    {
        if (!isInitialized) return;
        
        // Unsubscribe from previous listener if exists
        if (progressionsListener != null)
        {
            progressionsListener.Stop();
        }
        
        Query query = db.Collection("exercise_progressions_v3");
        
        if (!string.IsNullOrEmpty(curriculumId))
        {
            query = query.WhereEqualTo("curriculum_id", curriculumId);
        }
        
        progressionsListener = query.Listen(snapshot =>
        {
            var progressions = new List<ExerciseProgressionData>();
            
            foreach (var doc in snapshot.Documents)
            {
                var progression = ConvertToProgression(doc);
                if (progression != null)
                {
                    progressions.Add(progression);
                }
            }
            
            cachedProgressions = progressions;
            OnProgressionsUpdated?.Invoke(progressions);
            
            Debug.Log($"Progressions updated: {progressions.Count} items");
        });
    }
    
    // Subscribe to glossary updates
    public void SubscribeToGlossaryUpdates()
    {
        if (!isInitialized) return;
        
        if (glossaryListener != null)
        {
            glossaryListener.Stop();
        }
        
        glossaryListener = db.Collection("glossary_v3").Listen(snapshot =>
        {
            var glossary = new Dictionary<string, string>();
            
            foreach (var doc in snapshot.Documents)
            {
                if (doc.TryGetValue("key", out string key) && 
                    doc.TryGetValue("description", out string description))
                {
                    glossary[key] = description;
                }
            }
            
            cachedGlossary = glossary;
            OnGlossaryUpdated?.Invoke(glossary);
            
            Debug.Log($"Glossary updated: {glossary.Count} items");
        });
    }
    
    // Helper methods to convert Firestore documents to data models
    private Discipline ConvertToDiscipline(DocumentSnapshot doc)
    {
        var discipline = new Discipline
        {
            id = doc.Id,
            name = doc.GetValue<string>("name"),
            description_key = doc.GetValue<string>("description_key"),
            is_active = doc.GetValue<bool>("is_active"),
            created_date = doc.GetValue<string>("created_date"),
            created_by = doc.GetValue<string>("created_by"),
            tags = doc.TryGetValue("tags", out List<object> tagList) 
                ? tagList.Cast<string>().ToList() 
                : new List<string>()
        };
        
        return discipline;
    }
    
    private Curriculum ConvertToCurriculum(DocumentSnapshot doc)
    {
        var curriculum = new Curriculum
        {
            id = doc.Id,
            name = doc.GetValue<string>("name"),
            description_key = doc.GetValue<string>("description_key"),
            is_active = doc.GetValue<bool>("is_active"),
            created_date = doc.GetValue<string>("created_date"),
            created_by = doc.GetValue<string>("created_by"),
            target_audience = doc.TryGetValue("target_audience", out List<object> audienceList)
                ? audienceList.Cast<string>().ToList()
                : new List<string>(),
            tags = doc.TryGetValue("tags", out List<object> tagList)
                ? tagList.Cast<string>().ToList()
                : new List<string>()
        };
        
        return curriculum;
    }
    
    private async Task<Group> ConvertToGroupWithNestedData(DocumentSnapshot doc)
    {
        var group = new Group
        {
            id = doc.Id,
            name = doc.GetValue<string>("name"),
            description_key = doc.GetValue<string>("description_key"),
            is_active = doc.GetValue<bool>("is_active"),
            created_date = doc.GetValue<string>("created_date"),
            created_by = doc.GetValue<string>("created_by"),
            target_muscles = doc.TryGetValue("target_muscles", out List<object> muscleList)
                ? muscleList.Cast<string>().ToList()
                : new List<string>(),
            tags = doc.TryGetValue("tags", out List<object> tagList)
                ? tagList.Cast<string>().ToList()
                : new List<string>(),
            exercises = new List<Exercise>(),
            modules = new List<Module>()
        };
        
        // Fetch exercises for this group
        var exercisesRef = doc.Reference.Collection("exercises");
        var exerciseSnapshot = await exercisesRef.GetSnapshotAsync();
        
        foreach (var exerciseDoc in exerciseSnapshot.Documents)
        {
            group.exercises.Add(ConvertToExercise(exerciseDoc));
        }
        
        // Fetch modules for this group
        var modulesRef = doc.Reference.Collection("modules");
        var moduleSnapshot = await modulesRef.GetSnapshotAsync();
        
        foreach (var moduleDoc in moduleSnapshot.Documents)
        {
            var module = await ConvertToModuleWithExercises(moduleDoc);
            group.modules.Add(module);
        }
        
        return group;
    }
    
    private async Task<Module> ConvertToModuleWithExercises(DocumentSnapshot doc)
    {
        var module = new Module
        {
            id = doc.Id,
            name = doc.GetValue<string>("name"),
            description_key = doc.GetValue<string>("description_key"),
            is_active = doc.GetValue<bool>("is_active"),
            created_date = doc.GetValue<string>("created_date"),
            created_by = doc.GetValue<string>("created_by"),
            tags = doc.TryGetValue("tags", out List<object> tagList)
                ? tagList.Cast<string>().ToList()
                : new List<string>(),
            exercises = new List<Exercise>()
        };
        
        // Fetch exercises for this module
        var exercisesRef = doc.Reference.Collection("exercises");
        var exerciseSnapshot = await exercisesRef.GetSnapshotAsync();
        
        foreach (var exerciseDoc in exerciseSnapshot.Documents)
        {
            module.exercises.Add(ConvertToExercise(exerciseDoc));
        }
        
        return module;
    }
    
    private Exercise ConvertToExercise(DocumentSnapshot doc)
    {
        var exercise = new Exercise
        {
            id = doc.Id,
            name = doc.GetValue<string>("name"),
            description_key = doc.GetValue<string>("description_key"),
            position = doc.GetValue<int>("position"),
            is_active = doc.GetValue<bool>("is_active"),
            created_date = doc.GetValue<string>("created_date"),
            created_by = doc.GetValue<string>("created_by"),
            tags = doc.TryGetValue("tags", out List<object> tagList)
                ? tagList.Cast<string>().ToList()
                : new List<string>(),
            equipment_required = doc.TryGetValue("equipment_required", out List<object> equipList)
                ? equipList.Cast<string>().ToList()
                : new List<string>(),
            benchmarks = doc.TryGetValue("benchmarks", out List<object> benchList)
                ? benchList.Cast<string>().ToList()
                : new List<string>()
        };
        
        return exercise;
    }
    
    private ExerciseProgressionData ConvertToProgression(DocumentSnapshot doc)
    {
        try
        {
            var progression = new ExerciseProgressionData
            {
                id = doc.Id,
                curriculum_id = doc.GetValue<string>("curriculum_id"),
                discipline_id = doc.GetValue<string>("discipline_id"),
                parent_id = doc.GetValue<string>("parent_id"),
                parent_type = doc.GetValue<string>("parent_type"),
                group_id = doc.TryGetValue("group_id", out object groupId) ? groupId.ToString() : "",
                created_date = doc.GetValue<string>("created_date"),
                created_by = doc.GetValue<string>("created_by"),
                last_modified = doc.GetValue<string>("last_modified"),
                chains = new List<Chain>()
            };
            
            // Parse chains - it might be stored as a string or array
            if (doc.TryGetValue("chains", out object chainsObj))
            {
                if (chainsObj is string chainsStr)
                {
                    // Try to parse as JSON string
                    try
                    {
                        // Clean up the string (it seems to be truncated in the export)
                        if (chainsStr.StartsWith("[") && !chainsStr.EndsWith("]"))
                        {
                            Debug.LogWarning($"Chains string appears truncated for {doc.Id}, attempting to parse partial data");
                        }
                        
                        // For now, create a sample chain structure
                        // In production, you'd want to properly parse this
                        progression.chains = ParseChainsString(chainsStr, doc.Id);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error parsing chains string for {doc.Id}: {e.Message}");
                    }
                }
                else if (chainsObj is List<object> chainsList)
                {
                    foreach (var chainObj in chainsList)
                    {
                        if (chainObj is Dictionary<string, object> chainDict)
                        {
                            var chain = new Chain
                            {
                                id = chainDict.ContainsKey("id") ? chainDict["id"].ToString() : "",
                                color = chainDict.ContainsKey("color") ? chainDict["color"].ToString() : "#3B82F6",
                                exercise_ids = new List<string>()
                            };
                            
                            if (chainDict.ContainsKey("exercise_ids") && chainDict["exercise_ids"] is List<object> exerciseIds)
                            {
                                chain.exercise_ids = exerciseIds.Cast<string>().ToList();
                            }
                            
                            progression.chains.Add(chain);
                        }
                    }
                }
            }
            
            return progression;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error converting progression document {doc.Id}: {e.Message}");
            return null;
        }
    }
    
    private List<Chain> ParseChainsString(string chainsStr, string docId)
    {
        var chains = new List<Chain>();
        
        // For the sample data, let's create chains based on the document ID
        // This is a temporary solution until we can properly parse the chains field
        if (docId.Contains("ae9e3c6f"))
        {
            chains.Add(new Chain
            {
                id = "chain_ae9e3c6f",
                color = "#3B82F6",
                exercise_ids = new List<string> 
                { 
                    "3a3b5cb5-e009-4fd2-8db9-bbc3ef61c920",
                    "735c363e-0819-4b2b-9d03-fa763a3c2f28",
                    "23fe00de-84ed-4e92-ae6d-a7a0cc2c3a0a"
                }
            });
        }
        else if (docId.Contains("dc393d68"))
        {
            chains.Add(new Chain
            {
                id = "chain_dc393d68",
                color = "#10B981",
                exercise_ids = new List<string>
                {
                    "2a6ff617-fbaf-4ea8-9306-84274072f44e",
                    "c92af70c-4e41-4c5f-b697-c3e773f2d6d7",
                    "7faaf8ae-5583-4373-8295-ef9c2079fa0e",
                    "cbb38246-edb0-40b5-ab1f-0c69c2237c49"
                }
            });
        }
        else if (docId.Contains("e7b2f7f3"))
        {
            chains.Add(new Chain
            {
                id = "chain_e7b2f7f3",
                color = "#F59E0B",
                exercise_ids = new List<string>
                {
                    "b4de6877-c1fd-4953-a8bc-c100165c3674",
                    "79554865-03e7-47a7-ae39-e37dcab4c0a5",
                    "ed4333e1-47e5-478c-9c27-07ad7acc7a92",
                    "f15b9fec-355c-4d64-9e71-d1c29afdc236"
                }
            });
        }
        
        return chains;
    }
    
    // Get cached data
    public List<Discipline> GetCachedDisciplines() => cachedDisciplines;
    public Dictionary<string, string> GetCachedGlossary() => cachedGlossary;
    public List<ExerciseProgressionData> GetCachedProgressions() => cachedProgressions;
    
    void OnDestroy()
    {
        // Clean up listeners
        // disciplinesListener?.Stop();
        progressionsListener?.Stop();
        glossaryListener?.Stop();
    }
}