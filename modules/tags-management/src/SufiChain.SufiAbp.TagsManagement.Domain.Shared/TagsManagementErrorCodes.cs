namespace SufiChain.SufiAbp.TagsManagement;

public static class TagsManagementErrorCodes
{
    public const string Namespace = "TagsManagement";
    public const string TagAlreadyExists = Namespace + ":TagAlreadyExists";
    public const string TagNotFound = Namespace + ":TagNotFound";
    public const string MaxTagsPerEntityExceeded = Namespace + ":MaxTagsPerEntityExceeded";
    public const string MaxTagNameLengthExceeded = Namespace + ":MaxTagNameLengthExceeded";
}

