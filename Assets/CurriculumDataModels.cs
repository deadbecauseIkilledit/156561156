using System;
using System.Collections.Generic;

// Keep all data models in a namespace to avoid conflicts
namespace CurriculumSystem
{
    // Pure C# classes - no MonoBehaviour inheritance
    [Serializable]
    public class CurriculumExportData
    {
        public string exportDate;
        public string version;
        public Collections collections;
    }

    [Serializable]
    public class Collections
    {
        public List<Discipline> disciplines_v3;
        public List<GlossaryItem> glossary;
        public List<ExerciseProgressionData> exercise_progressions;
    }

    [Serializable]
    public class Discipline
    {
        public string id;
        public string name;
        public string description_key;
        public bool is_active;
        public string created_date;
        public string created_by;
        public List<string> tags;
        public List<Curriculum> curriculums;
    }

    [Serializable]
    public class Curriculum
    {
        public string id;
        public string name;
        public string description_key;
        public bool is_active;
        public string created_date;
        public string created_by;
        public List<string> target_audience;
        public List<string> tags;
        public List<Group> groups;
    }

    [Serializable]
    public class Group
    {
        public string id;
        public string name;
        public string description_key;
        public bool is_active;
        public string created_date;
        public string created_by;
        public List<string> target_muscles;
        public List<string> tags;
        public List<Exercise> exercises;
        public List<Module> modules;
    }

    [Serializable]
    public class Module
    {
        public string id;
        public string name;
        public string description_key;
        public bool is_active;
        public string created_date;
        public string created_by;
        public List<string> tags;
        public List<Exercise> exercises;
    }

    [Serializable]
    public class Exercise
    {
        public string id;
        public string name;
        public string description_key;
        public int position;
        public bool is_active;
        public string created_date;
        public string created_by;
        public List<string> tags;
        public List<string> equipment_required;
        public List<string> benchmarks;
    }

    [Serializable]
    public class ExerciseProgressionData
    {
        public string id;
        public string curriculum_id;
        public string discipline_id;
        public string parent_id;
        public string parent_type;
        public string group_id;
        public string created_date;
        public string created_by;
        public string last_modified;
        public List<Chain> chains;
    }

    [Serializable]
    public class Chain
    {
        public string id;
        public string color;
        public List<string> exercise_ids;
    }

    [Serializable]
    public class GlossaryItem
    {
        public string id;
        public string key;
        public string description;
        public string created_date;
        public string created_by;
    }
}