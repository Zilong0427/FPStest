using System;

namespace EnhancedHierarchy {

    [Flags]
    internal enum EntryMode {
        ScriptingError = 256,
        ScriptingWarning = 512,
        ScriptingLog = 1024
    }

    internal enum MiniLabelType {
        None = 0,
        Tag = 1,
        Layer = 2,
        TagOrLayer = 3,
        LayerOrTag = 4,
    }

    internal enum DrawType {
        Enable = 0,
        Static = 1,
        Lock = 2,
        Icon = 3,
        ApplyPrefab = 4,
        Tag = 5,
        Layer = 6,
        None = 7
    }

    internal enum StripType {
        None = 0,
        Color = 1,
        Lines = 2,
        ColorAndLines = Color | Lines
    }

    internal enum ChildrenChangeMode {
        ObjectAndChildren = 0,
        ObjectOnly = 1,
        Ask = 2,
    }
}