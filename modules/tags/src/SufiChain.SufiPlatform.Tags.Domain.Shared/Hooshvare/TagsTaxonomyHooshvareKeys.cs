namespace SufiChain.SufiPlatform.Tags.Hooshvare;

public static class TagsTaxonomyHooshvareKeys
{
    public const string Key = "SufiTags:Taxonomy";
    public const string LocalizationResourceName = "Tags";
    public const int EntityVersion = 1;

    public static class Tools
    {
        public const string Search = "tags.search";
        public const string Get = "tags.get";
        public const string ListByScope = "tags.list_by_scope";
        public const string GetLinksByTag = "tags.get_links_by_tag";
        public const string GetTagsByEntity = "tags.get_tags_by_entity";
    }

    public static class Context
    {
        public const string Scope = "scope";
        public const string TagId = "tagId";
        public const string EntityType = "entityType";
        public const string EntityId = "entityId";
    }

    public static class Shortcuts
    {
        public const string SearchScope = "SearchScope";
        public const string ExplainSelection = "ExplainSelection";
    }
}
