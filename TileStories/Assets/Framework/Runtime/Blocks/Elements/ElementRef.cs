namespace TileStories
{
    // Where a reference row points (_3.3 R17.8): a row of THIS block's Page (scope page), of another block of the same POI (scope block,
    // Block = that block's key) or of the POI's shared elements (scope shared). Row is the target row's id (el_N). One level only: the
    // target must be an element, never another reference (ElementRefRule). Stored on the reference row itself (ElementRow.SetRef).
    public readonly struct ElementRef
    {
        // One of ElementOptions.Scopes (an unknown one resolves as Broken, never guessed)
        public readonly string Scope;
        // Scope block only: the target block's key; blank for the other scopes
        public readonly string Block;
        public readonly string Row;

        public ElementRef(string scope, string block, string row)
        {
            Scope = scope ?? "";
            Block = block ?? "";
            Row = row ?? "";
        }

        // A link to a row of the holder's own Page (what the Preview's linked rows are, R17.1)
        public static ElementRef ToPage(string row) => new(ElementOptions.ScopePage, "", row);

        // A link to a row of another block of the same POI
        public static ElementRef ToBlock(string block, string row) => new(ElementOptions.ScopeBlock, block, row);

        // A link to a row of the POI's shared elements
        public static ElementRef ToShared(string row) => new(ElementOptions.ScopeShared, "", row);
    }
}
