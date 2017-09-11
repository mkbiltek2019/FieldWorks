﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LanguageExplorer.Areas.Lexicon {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class LexiconResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal LexiconResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LanguageExplorer.Areas.Lexicon.LexiconResources", typeof(LexiconResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;root&gt;
        ///	&lt;parameters toolId=&quot;bulkEditEntriesOrSenses&quot; filterBar=&quot;true&quot; bulkEdit=&quot;true&quot; selectColumn=&quot;true&quot; bulkEditListItemsClasses=&quot;LexEntry,LexSense,LexEntryRef,LexPronunciation,MoForm,LexExampleSentence,CmTranslation&quot; bulkEditListItemsGhostFields=&quot;LexDb.AllPossiblePronunciations,LexDb.AllPossibleAllomorphs,LexDb.AllExampleSentenceTargets,LexDb.AllExampleTranslationTargets,LexDb.AllComplexEntryRefPropertyTargets&quot;&gt;
        ///		&lt;includeColumns /&gt;
        ///	&lt;/parameters&gt;
        ///	&lt;overrides&gt; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string BulkEditEntriesOrSensesToolParameters {
            get {
                return ResourceManager.GetString("BulkEditEntriesOrSensesToolParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Delete this Sense and any Subsenses.
        /// </summary>
        internal static string DeleteSenseAndSubsenses {
            get {
                return ResourceManager.GetString("DeleteSenseAndSubsenses", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to _Entry....
        /// </summary>
        internal static string Entry {
            get {
                return ResourceManager.GetString("Entry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create a new lexical entry..
        /// </summary>
        internal static string Entry_Tooltip {
            get {
                return ResourceManager.GetString("Entry_Tooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;!-- this file lists special instructions for XDE nodes. Currently, all that you are allowed to say is
        ///that a node is not visible. You do that by listing the node here. the id of the node must match the XDE
        ///node that would otherwise produce a slice which you want to hide. note that most XDE nodes
        ///do not otherwise have an ID; you usually have to add 1 before you can reference it in this kind of document.--&gt;
        ///&lt;SliceFilter&gt;
        ///	&lt;node id=&quot;MoAffixAllomorphInflectionClass [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string HideAdvancedFeatureFields {
            get {
                return ResourceManager.GetString("HideAdvancedFeatureFields", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Insert A_llomorph.
        /// </summary>
        internal static string Insert_Allomorph {
            get {
                return ResourceManager.GetString("Insert_Allomorph", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add an allomorph to this entry.
        /// </summary>
        internal static string Insert_Allomorph_Tooltip {
            get {
                return ResourceManager.GetString("Insert_Allomorph_Tooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Insert _Pronunciation.
        /// </summary>
        internal static string Insert_Pronunciation {
            get {
                return ResourceManager.GetString("Insert_Pronunciation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add a pronunciation field to this entry..
        /// </summary>
        internal static string Insert_Pronunciation_Tooltip {
            get {
                return ResourceManager.GetString("Insert_Pronunciation_Tooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Insert _Sense.
        /// </summary>
        internal static string Insert_Sense {
            get {
                return ResourceManager.GetString("Insert_Sense", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Insert Sound or Movie _File.
        /// </summary>
        internal static string Insert_Sound_Or_Movie_File {
            get {
                return ResourceManager.GetString("Insert_Sound_Or_Movie_File", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add a sound or movie file to the pronunciation, creating the pronunciation if necessary..
        /// </summary>
        internal static string Insert_Sound_Or_Movie_File_Tooltip {
            get {
                return ResourceManager.GetString("Insert_Sound_Or_Movie_File_Tooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Insert Subsense (in sense).
        /// </summary>
        internal static string Insert_Subsense {
            get {
                return ResourceManager.GetString("Insert_Subsense", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Insert a new subsense..
        /// </summary>
        internal static string Insert_Subsense_Tooltip {
            get {
                return ResourceManager.GetString("Insert_Subsense_Tooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Insert _Variant.
        /// </summary>
        internal static string Insert_Variant {
            get {
                return ResourceManager.GetString("Insert_Variant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add a Variant Form to this entry.
        /// </summary>
        internal static string Insert_Variant_Tooltip {
            get {
                return ResourceManager.GetString("Insert_Variant_Tooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Insert a new sense..
        /// </summary>
        internal static string InsertSenseToolTip {
            get {
                return ResourceManager.GetString("InsertSenseToolTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to _Find.
        /// </summary>
        internal static string ks_Find {
            get {
                return ResourceManager.GetString("ks_Find", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to _Merge.
        /// </summary>
        internal static string ks_Merge {
            get {
                return ResourceManager.GetString("ks_Merge", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The two entries have been merged..
        /// </summary>
        internal static string ksEntriesHaveBeenMerged {
            get {
                return ResourceManager.GetString("ksEntriesHaveBeenMerged", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Merge Entry.
        /// </summary>
        internal static string ksMergeEntry {
            get {
                return ResourceManager.GetString("ksMergeEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Merge Report.
        /// </summary>
        internal static string ksMergeReport {
            get {
                return ResourceManager.GetString("ksMergeReport", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Redo Merge Entry.
        /// </summary>
        internal static string ksRedoMergeEntry {
            get {
                return ResourceManager.GetString("ksRedoMergeEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Undo Merge Entry.
        /// </summary>
        internal static string ksUndoMergeEntry {
            get {
                return ResourceManager.GetString("ksUndoMergeEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lexeme Form has components.
        /// </summary>
        internal static string Lexeme_Form_Has_Components {
            get {
                return ResourceManager.GetString("Lexeme_Form_Has_Components", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This Lexeme Form contains several morphemes. Add the fields for me to specify the components and type of complex form..
        /// </summary>
        internal static string Lexeme_Form_Has_Components_Tooltip {
            get {
                return ResourceManager.GetString("Lexeme_Form_Has_Components_Tooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lexeme Form is a variant.
        /// </summary>
        internal static string Lexeme_Form_Is_A_Variant {
            get {
                return ResourceManager.GetString("Lexeme_Form_Is_A_Variant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This Lexeme Form is related to another form as a variant. Add the fields for me to specify the related form(s) and type of variant..
        /// </summary>
        internal static string Lexeme_Form_Is_A_Variant_Tooltip {
            get {
                return ResourceManager.GetString("Lexeme_Form_Is_A_Variant_Tooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;!-- Please increment BrowseViewer.kBrowseViewVersion when you change these specs,
        /// so that XmlBrowseViewBaseVc can invalidate obsoleted columns that have been saved in each current control&apos;s ColumnList --&gt;
        ///&lt;columns generate=&quot;childPartsForParentLayouts&quot;&gt;
        ///	&lt;!-- These columns typically appear installed in entries browse type tools in this order. --&gt;
        ///	&lt;column layout=&quot;EntryHeadwordForEntry&quot; label=&quot;Headword&quot; ws=&quot;$ws=vernacular&quot; width=&quot;72000&quot; sortmethod=&quot;FullSortKey&quot; cansortbylength=&quot;true&quot; visibility=&quot;always&quot; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LexiconBrowseDialogColumnDefinitions {
            get {
                return ResourceManager.GetString("LexiconBrowseDialogColumnDefinitions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;overrides&gt;
        ///	&lt;column layout=&quot;CitationFormForEntry&quot; visibility=&quot;menu&quot; /&gt;
        ///	&lt;column layout=&quot;GrammaticalInfoAbbrForSense&quot; visibility=&quot;menu&quot; /&gt;
        ///	&lt;column layout=&quot;DomainsOfSensesForSense&quot; visibility=&quot;menu&quot; /&gt;
        ///&lt;/overrides&gt;.
        /// </summary>
        internal static string LexiconBrowseOverrides {
            get {
                return ResourceManager.GetString("LexiconBrowseOverrides", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;parameters area=&quot;lexicon&quot; id=&quot;EntriesList&quot; clerk=&quot;entries&quot; field=&quot;Entries&quot; filterBar=&quot;true&quot; altTitleId=&quot;LexEntry-Plural&quot; /&gt;.
        /// </summary>
        internal static string LexiconBrowseParameters {
            get {
                return ResourceManager.GetString("LexiconBrowseParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;parameters altTitleId=&quot;LexSense-Classified&quot; persistContext=&quot;ClassDict&quot; backColor=&quot;White&quot; layout=&quot;classifiedDict&quot; layoutProperty=&quot;ClassifiedDictionaryPublicationLayout&quot; editable=&quot;false&quot; allowInsertDeleteRecord=&quot;false&quot; msgBoxTrigger=&quot;ClassifiedDictionary-Intro&quot; configureObjectName=&quot;Classified Dictionary&quot;&gt;
        ///	&lt;elementDisplayCondition field=&quot;ReferringSenses&quot; lengthatleast=&quot;1&quot;/&gt;
        ///	&lt;configureLayouts&gt;
        ///		&lt;layoutType label=&quot;Classified Dictionary&quot; layout=&quot;classifiedDict&quot;&gt;
        ///		 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LexiconClassifiedDictionaryParameters {
            get {
                return ResourceManager.GetString("LexiconClassifiedDictionaryParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;!-- The following configureLayouts node is only required to help migrate old configurations to the new format --&gt;
        ///&lt;configureLayouts&gt;
        ///	&lt;layoutType label=&quot;Stem-based (complex forms as main entries)&quot; layout=&quot;publishStem&quot;&gt;
        ///		&lt;configure class=&quot;LexEntry&quot; label=&quot;Main Entry&quot; layout=&quot;publishStemEntry&quot;/&gt;
        ///		&lt;configure class=&quot;LexEntry&quot; label=&quot;Minor Entry&quot; layout=&quot;publishStemMinorEntry&quot; hideConfig=&quot;true&quot;/&gt;
        ///	&lt;/layoutType&gt;
        ///	&lt;layoutType label=&quot;Root-based (complex forms as sub [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LexiconDictionaryConfigureLayouts {
            get {
                return ResourceManager.GetString("LexiconDictionaryConfigureLayouts", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;parameters clerk=&quot;entries&quot; altTitleId=&quot;LexEntry-Plural&quot; persistContext=&quot;Dict&quot; backColor=&quot;White&quot; layout=&quot;&quot; layoutProperty=&quot;DictionaryPublicationLayout&quot; layoutSuffix=&quot;Preview&quot; editable=&quot;false&quot; configureObjectName=&quot;Dictionary&quot; /&gt;.
        /// </summary>
        internal static string LexiconDictionaryToolParameters {
            get {
                return ResourceManager.GetString("LexiconDictionaryToolParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;parameters id=&quot;DictionaryPubPreview&quot; layout=&quot;publishStem&quot; layoutProperty=&quot;DictionaryPublicationLayout&quot; layoutSuffix=&quot;Preview&quot; editable=&quot;false&quot; configureObjectName=&quot;Dictionary&quot; viewTypeLabelKey=&quot;ksDictionaryView&quot; emptyTitleId=&quot;No-LexEntries&quot;&gt;
        ///	&lt;configureLayouts&gt;
        ///		&lt;layoutType label=&quot;Stem-based (complex forms as main entries)&quot; layout=&quot;publishStem&quot;&gt;
        ///			&lt;configure class=&quot;LexEntry&quot; label=&quot;Main Entry&quot; layout=&quot;publishStemEntry&quot;/&gt;
        ///			&lt;configure class=&quot;LexEntry&quot; label=&quot;M [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LexiconEditRecordDocViewParameters {
            get {
                return ResourceManager.GetString("LexiconEditRecordDocViewParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;parameters PaneBarGroupId=&quot;PaneBar-LexicalDetail&quot; persistContext=&quot;normalEdit&quot; suppressInfoBar=&quot;ifNotFirst&quot; layout=&quot;Normal&quot; treeBarAvailability=&quot;NotAllowed&quot; emptyTitleId=&quot;No-LexEntries&quot; printLayout=&quot;publishStem&quot; /&gt;
        ///.
        /// </summary>
        internal static string LexiconEditRecordEditViewParameters {
            get {
                return ResourceManager.GetString("LexiconEditRecordEditViewParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;parameters id=&quot;lexentryMatchList&quot; listItemsClass=&quot;LexEntry&quot; filterBar=&quot;false&quot; treeBarAvailability=&quot;NotAllowed&quot; defaultCursor=&quot;Arrow&quot; hscroll=&quot;true&quot; altTitleId=&quot;LexEntry-Plural&quot; editable=&quot;false&quot;&gt;
        ///	&lt;columns&gt;
        ///		&lt;column label=&quot;Headword&quot; sortmethod=&quot;FullSortKey&quot; ws=&quot;$ws=best vernoranal&quot; editable=&quot;false&quot; width=&quot;68000&quot; layout=&quot;EntryHeadwordForFindEntry&quot; /&gt;
        ///		&lt;column label=&quot;Lexeme Form&quot; visibility=&quot;menu&quot; common=&quot;true&quot; sortmethod=&quot;MorphSortKey&quot; ws=&quot;$ws=best vernoranal&quot; ed [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string MatchingEntriesParameters {
            get {
                return ResourceManager.GetString("MatchingEntriesParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to _Merge with entry....
        /// </summary>
        internal static string Merge_With_Entry {
            get {
                return ResourceManager.GetString("Merge_With_Entry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Merge current lexical entry into another entry..
        /// </summary>
        internal static string Merge_With_Entry_Tooltip {
            get {
                return ResourceManager.GetString("Merge_With_Entry_Tooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;root&gt;
        ///	&lt;recordeditview&gt;
        ///		&lt;parameters treeBarAvailability=&quot;Required&quot; layout=&quot;RDE&quot; msgBoxTrigger=&quot;CategorizedEntry-Intro&quot; allowInsertDeleteRecord=&quot;false&quot; /&gt;
        ///	&lt;/recordeditview&gt;
        ///	&lt;recordbrowseview&gt;
        ///		&lt;parameters id=&quot;wordList&quot; filterBar=&quot;false&quot; forceReloadListOnInitOrChangeRoot=&quot;true&quot; editRowModelClass=&quot;LexSense&quot; editRowAssembly=&quot;SIL.LCModel.dll&quot; editRowClass=&quot;SIL.LCModel.ILexSense&quot; editRowSaveMethod=&quot;RDENewSense&quot; editRowMergeMethod=&quot;RDEMergeSense&quot; ShowOwnerShortna [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string RapidDataEntryToolParameters {
            get {
                return ResourceManager.GetString("RapidDataEntryToolParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;parameters toolId=&quot;reversalBulkEditReversalEntries&quot; clerk=&quot;AllReversalEntries&quot; filterBar=&quot;true&quot; bulkEdit=&quot;true&quot; bulkEditListItemsClasses=&quot;ReversalIndexEntry&quot; selectColumn=&quot;true&quot; altTitleId=&quot;ReversalIndexEntry-Plural&quot; ShowOwnerShortname=&quot;true&quot; &gt;
        ///	&lt;enableBulkEditTabs enableBEListChoice=&quot;true&quot; enableBEBulkCopy=&quot;true&quot; enableBEClickCopy=&quot;true&quot; enableBEProcess=&quot;true&quot; enableBEFindReplace=&quot;true&quot; enableBEOther=&quot;true&quot; /&gt;
        ///	&lt;columns&gt;
        ///		&lt;!-- NB: If you add a new column and it [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ReversalBulkEditReversalEntriesToolParameters {
            get {
                return ResourceManager.GetString("ReversalBulkEditReversalEntriesToolParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;root&gt;
        ///	&lt;docview&gt;
        ///		&lt;parameters clerk=&quot;AllReversalEntries&quot;  persistContext=&quot;Reversal&quot; backColor=&quot;White&quot; layout=&quot;&quot; layoutProperty=&quot;ReversalIndexPublicationLayout&quot; layoutSuffix=&quot;Preview&quot; editable=&quot;false&quot; configureObjectName=&quot;ReversalIndex&quot; altTitleId=&quot;ReversalIndexEntry-Plural&quot; &gt;
        ///			&lt;!-- The following configureLayouts node is required only to help migrate old configurations to the new format --&gt;
        ///			&lt;configureLayouts&gt;
        ///				&lt;layoutType label=&quot;All Reversal Indexes&quot; la [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ReversalEditCompleteToolParameters {
            get {
                return ResourceManager.GetString("ReversalEditCompleteToolParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show Dictionary Preview.
        /// </summary>
        internal static string Show_DictionaryPubPreview {
            get {
                return ResourceManager.GetString("Show_DictionaryPubPreview", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Display the dictionary entry for this lexeme above the Lexicon Edit pane..
        /// </summary>
        internal static string Show_DictionaryPubPreview_ToolTip {
            get {
                return ResourceManager.GetString("Show_DictionaryPubPreview_ToolTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show Entry in Concordance.
        /// </summary>
        internal static string Show_Entry_In_Concordance {
            get {
                return ResourceManager.GetString("Show_Entry_In_Concordance", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show Unused Items.
        /// </summary>
        internal static string Show_Unused_Items {
            get {
                return ResourceManager.GetString("Show_Unused_Items", resourceCulture);
            }
        }
    }
}
