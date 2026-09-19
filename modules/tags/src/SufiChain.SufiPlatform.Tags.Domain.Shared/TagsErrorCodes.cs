namespace SufiChain.SufiPlatform.Tags;

public static class TagsErrorCodes
{
    public const string Namespace = "Tags";
    public const string TagAlreadyExists = Namespace + ":TagAlreadyExists";
    public const string TagNotFound = Namespace + ":TagNotFound";
    public const string PredicateRequiresRelationWorkflow = Namespace + ":PredicateRequiresRelationWorkflow";
    public const string InvalidRelationDefinition = Namespace + ":InvalidRelationDefinition";
    public const string RelationVersionConflict = Namespace + ":RelationVersionConflict";
    public const string RelationRequestConflict = Namespace + ":RelationRequestConflict";
    public const string RelationHistoryLimit = Namespace + ":RelationHistoryLimit";
    public const string RelationRetracted = Namespace + ":RelationRetracted";
    public const string RelationCommandDenied = Namespace + ":RelationCommandDenied";
    public const string RelationReceiptConflict = Namespace + ":RelationReceiptConflict";
    public const string MaxTagsPerEntityExceeded = Namespace + ":MaxTagsPerEntityExceeded";
    public const string MaxTagNameLengthExceeded = Namespace + ":MaxTagNameLengthExceeded";
}

