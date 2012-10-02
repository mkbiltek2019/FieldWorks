﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using SIL.CoreImpl;
using SIL.FieldWorks.Common.COMInterfaces;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.FDO.DomainServices;
using SIL.FieldWorks.IText.FlexInterlinModel;

namespace SIL.FieldWorks.IText
{
	public partial class LinguaLinksImport
	{
		//this delegate is used for alerting the user of new writing systems found in the import
		private delegate DialogResult ShowDialogAboveProgressbarDelegate(IThreadedProgress progress, string text, string title, MessageBoxButtons buttons);


		private static Dictionary<string, ILgWritingSystem> s_wsMapper = new Dictionary<string, ILgWritingSystem>();

		/// <summary>
		/// This method will display a message box above the progress dialog.
		/// </summary>
		/// <param name="progress"></param>
		/// <param name="text"></param>
		/// <param name="title"></param>
		/// <param name="buttons"></param>
		/// <returns></returns>
		private static DialogResult ShowDialogAboveProgressbar(IThreadedProgress progress, string text, string title, MessageBoxButtons buttons)
		{
			return MessageBox.Show(progress.Form,
								   text,
								   title,
								   buttons,
								   MessageBoxIcon.Warning);
		}

		internal class TextCreationParams
		{
			internal Interlineartext InterlinText;
			internal FdoCache Cache;
			internal IThreadedProgress Progress;
			internal Func<FdoCache, Interlineartext, ILgWritingSystemFactory, IThreadedProgress, bool> CheckAndAddLanguages;
		}

		/// <summary>
		/// This method will create a new Text document from the given BIRD format Interlineartext.
		/// </summary>
		/// <param name="newText">The text to populate, could be set to null.</param>
		/// <param name="interlinText">The Interlineartext object from the BIRDDocument read from the xml file</param>
		/// <param name="cache">The FdoCache to use when populating the text object</param>
		/// <param name="progress">The progress bar, used to show error dialogs above other windows.</param>
		private static bool PopulateTextFromBIRDDoc(ref FDO.IText newText, TextCreationParams textParams)
		{
			Interlineartext interlinText = textParams.InterlinText;
			FdoCache cache = textParams.Cache;
			IThreadedProgress progress = textParams.Progress;
			if (textParams.CheckAndAddLanguages == null)
				textParams.CheckAndAddLanguages = CheckAndAddLanguagesInternal;

			ILgWritingSystemFactory wsFactory = cache.WritingSystemFactory;
			char space = ' ';
			//handle the languages(writing systems) section alerting the user if new writing systems are encountered
			if (!textParams.CheckAndAddLanguages(cache, interlinText, wsFactory, progress))
				return false;

			//handle the header(info or meta) information
			SetTextMetaAndMedia(cache, interlinText, wsFactory, newText);

			//create all the paragraphs
			foreach (var paragraph in interlinText.paragraphs)
			{
				if (newText.ContentsOA == null)
				{
					newText.ContentsOA = cache.ServiceLocator.GetInstance<IStTextFactory>().Create();
				}
				IStTxtPara newTextPara = newText.ContentsOA.AddNewTextPara("");
				int offset = 0;
				if (paragraph.phrases == null)
				{
					continue;
				}
				foreach (var phrase in paragraph.phrases)
				{
					ICmObject oldSegment = null;
					//Try and locate a segment with this Guid.
					if (!String.IsNullOrEmpty(phrase.guid))
					{
						if (cache.ServiceLocator.ObjectRepository.TryGetObject(new Guid(phrase.guid), out oldSegment))
						{
							//We aren't merging, but we have this guid in our system, ignore the file Guid
							oldSegment = cache.ServiceLocator.GetInstance<ISegmentFactory>().Create(newTextPara, offset);
						}
						else
						{
							//The segment is identified by a Guid, but apparently we don't have it in our current document, so make one with the guid
							oldSegment = cache.ServiceLocator.GetInstance<ISegmentFactory>().Create(newTextPara, offset, cache,
																									new Guid(phrase.guid));
						}
					}
					//set newSegment to the old, or create a brand new one.
					ISegment newSegment = oldSegment as ISegment ?? cache.ServiceLocator.GetInstance<ISegmentFactory>().Create(newTextPara, offset);
					var tsStrFactory = cache.ServiceLocator.GetInstance<ITsStrFactory>();
					//Fill in the ELAN time information if it is present.
					AddELANInfoToSegment(cache, phrase, newSegment);
					ITsString phraseText = null;
					bool textInFile = false;
					//Add all of the data from <item> elements into the segment.
					AddSegmentItemData(cache, wsFactory, phrase, newSegment, tsStrFactory, ref textInFile, ref phraseText);
					bool lastWasWord = false;
					if (phrase.WordsContent != null && phrase.WordsContent.Words != null)
					{
						foreach (var word in phrase.WordsContent.Words)
						{
							//If the text of the phrase was not given in the document build it from the words.
							if (!textInFile)
							{
								UpdatePhraseTextForWordItems(wsFactory, tsStrFactory, ref phraseText, word, ref lastWasWord, space);
							}
							AddWordToSegment(newSegment, word, tsStrFactory);
						}
					}
					UpdateParagraphTextForPhrase(newTextPara, ref offset, phraseText);
				}
			}
			return true;
		}

		private static bool MergeTextWithBIRDDoc(ref FDO.IText newText, TextCreationParams textParams)
		{
			Interlineartext interlinText = textParams.InterlinText;
			FdoCache cache = textParams.Cache;
			IThreadedProgress progress = textParams.Progress;
			if (textParams.CheckAndAddLanguages == null)
				textParams.CheckAndAddLanguages = CheckAndAddLanguagesInternal;

			ILgWritingSystemFactory wsFactory = cache.WritingSystemFactory;
			char space = ' ';
			//handle the languages(writing systems) section alerting the user if new writing systems are encountered
			if (!textParams.CheckAndAddLanguages(cache, interlinText, wsFactory, progress))
				return false;

			//handle the header(info or meta) information as well as any media-files sections
			SetTextMetaAndMedia(cache, interlinText, wsFactory, newText);

			IStText oldContents = newText.ContentsOA;
			IStText newContents = null;
			//create all the paragraphs NOTE: Currently the paragraph guids are being ignored, this might be wrong.
			foreach (var paragraph in interlinText.paragraphs)
			{
				if (newContents == null)
				{
					newContents = cache.ServiceLocator.GetInstance<IStTextFactory>().Create();
					newText.ContentsOA = newContents;
				}
				IStTxtPara newTextPara = newContents.AddNewTextPara("");
				int offset = 0;
				if (paragraph.phrases == null)
				{
					continue;
				}
				foreach (var phrase in paragraph.phrases)
				{
					ICmObject oldSegment = null;
					//Try and locate a segment with this Guid.
					if(!String.IsNullOrEmpty(phrase.guid))
					{
						if (cache.ServiceLocator.ObjectRepository.TryGetObject(new Guid(phrase.guid), out oldSegment))
						{
							if (oldSegment as ISegment != null) //The segment matches, add it into our paragraph.
								newTextPara.SegmentsOS.Add(oldSegment as ISegment);
							else if(oldSegment == null) //The segment is identified by a Guid, but apparently we don't have it in our current document, so make one
								oldSegment = cache.ServiceLocator.GetInstance<ISegmentFactory>().Create(newTextPara, offset, cache, new Guid(phrase.guid));
							else //The Guid is in use, but not by a segment. This is bad.
							{
								return false;
							}
						}
					}
					//set newSegment to the old, or create a brand new one.
					ISegment newSegment = oldSegment as ISegment ?? cache.ServiceLocator.GetInstance<ISegmentFactory>().Create(newTextPara, offset);
					var tsStrFactory = cache.ServiceLocator.GetInstance<ITsStrFactory>();
					//Fill in the ELAN time information if it is present.
					AddELANInfoToSegment(cache, phrase, newSegment);

					ITsString phraseText = null;
					bool textInFile = false;
					//Add all of the data from <item> elements into the segment.
					AddSegmentItemData(cache, wsFactory, phrase, newSegment, tsStrFactory, ref textInFile, ref phraseText);

					bool lastWasWord = false;
					if (phrase.WordsContent != null)
					{
						foreach (var word in phrase.WordsContent.Words)
						{
							//If the text of the phrase was not found in a "txt" item for this segment then build it from the words.
							if (!textInFile)
							{
								UpdatePhraseTextForWordItems(wsFactory, tsStrFactory, ref phraseText, word, ref lastWasWord, space);
							}
							MergeWordToSegment(newSegment, word, tsStrFactory);
						}
					}
					UpdateParagraphTextForPhrase(newTextPara, ref offset, phraseText);
				}
			}
			return true;
		}

		/// <summary>
		/// This method will update the newTextPara param appending the phraseText and possibly modifying the segment ending
		/// to add an end of segment character. The offset parameter will be set to the value where a following segment would start
		/// from.
		/// </summary>
		/// <param name="newTextPara"></param>
		/// <param name="offset"></param>
		/// <param name="phraseText"></param>
		private static void UpdateParagraphTextForPhrase(IStTxtPara newTextPara, ref int offset, ITsString phraseText)
		{
			if (phraseText != null && phraseText.Length > 0)
			{
				offset += phraseText.Length;
				var bldr = newTextPara.Contents.GetBldr();
				var oldText = (bldr.Text ?? "").Trim();
				if (oldText.Length > 0 && !TsStringUtils.IsEndOfSentenceChar(oldText[oldText.Length - 1], LgGeneralCharCategory.kccPo))
				{
					// 'segment' does not end with recognizable EOS character. Add our special one.
					bldr.Replace(bldr.Length, bldr.Length, "\x00A7", null);
				}
				// Insert a space between phrases unless there is already one
				if (bldr.Length > 0 && phraseText.Text[0] != ' ' && bldr.Text[bldr.Length - 1] != ' ')
					bldr.Replace(bldr.Length, bldr.Length, " ", null);
				bldr.ReplaceTsString(bldr.Length, bldr.Length, phraseText);
				newTextPara.Contents = bldr.GetString();
			}
		}

		private static ILgWritingSystem GetWsEngine(ILgWritingSystemFactory wsFactory, string langCode)
		{
			ILgWritingSystem result;
			if (s_wsMapper.TryGetValue(langCode, out result))
				return result;

			return wsFactory.get_Engine(langCode);
		}

		/// <summary>
		/// This method will update the phraseText ref item with the contents of the item entries under the word
		/// </summary>
		/// <param name="wsFactory"></param>
		/// <param name="tsStrFactory"></param>
		/// <param name="phraseText"></param>
		/// <param name="word"></param>
		/// <param name="lastWasWord"></param>
		/// <param name="space"></param>
		private static void UpdatePhraseTextForWordItems(ILgWritingSystemFactory wsFactory, ITsStrFactory tsStrFactory, ref ITsString phraseText, Word word, ref bool lastWasWord, char space)
		{
			bool isWord = false;
			foreach (var item in word.Items)
			{
				switch (item.type)
				{
					case "txt": //intentional fallthrough
						isWord = true;
						goto case "punct";
					case "punct":
						ITsString wordString = tsStrFactory.MakeString(item.Value,
							GetWsEngine(wsFactory, item.lang).Handle);
						if (phraseText == null)
						{
							phraseText = wordString;
						}
						else
						{
							var phraseBldr = phraseText.GetBldr();
							if (lastWasWord && isWord) //two words next to each other deserve a space between
							{
								phraseBldr.ReplaceTsString(phraseText.Length, phraseText.Length,
								   tsStrFactory.MakeString("" + space, GetWsEngine(wsFactory, item.lang).Handle));
							}
							else if (!isWord) //handle punctuation
							{
								wordString = GetSpaceAdjustedPunctString(wsFactory, tsStrFactory,
									item, wordString, space, lastWasWord);
							}
							phraseBldr.ReplaceTsString(phraseBldr.Length, phraseBldr.Length, wordString);
							phraseText = phraseBldr.GetString();
						}
						lastWasWord = isWord;
						isWord = false;
						break;
				}
			}
		}

		/// <summary>
		/// Add all the data from items in the FLExText file into their proper spots in the segment.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="wsFactory"></param>
		/// <param name="phrase"></param>
		/// <param name="newSegment"></param>
		/// <param name="tsStrFactory"></param>
		/// <param name="textInFile">This reference boolean indicates if there was a text item in the phrase</param>
		/// <param name="phraseText">This reference string will be filled with the contents of the "txt" item in the phrase if it is there</param>
		private static void AddSegmentItemData(FdoCache cache, ILgWritingSystemFactory wsFactory, Phrase phrase, ISegment newSegment, ITsStrFactory tsStrFactory, ref bool textInFile, ref ITsString phraseText)
		{
			if (phrase.Items != null)
			{
				foreach (var item in phrase.Items)
				{
					switch (item.type)
					{
						case "reference-label":
							newSegment.Reference = tsStrFactory.MakeString(item.Value,
								GetWsEngine(wsFactory, item.lang).Handle);
							break;
						case "gls":
							newSegment.FreeTranslation.set_String(GetWsEngine(wsFactory, item.lang).Handle, item.Value);
							break;
						case "lit":
							newSegment.LiteralTranslation.set_String(GetWsEngine(wsFactory, item.lang).Handle, item.Value);
							break;
						case "note":
							INote note = cache.ServiceLocator.GetInstance<INoteFactory>().Create();
							newSegment.NotesOS.Add(note);
							note.Content.set_String(GetWsEngine(wsFactory, item.lang).Handle, item.Value);
							break;
						case "txt":
							phraseText = tsStrFactory.MakeString(item.Value, GetWsEngine(wsFactory, item.lang).Handle);
							textInFile = true;
							break;
					}
				}
			}
		}

		private static void AddELANInfoToSegment(FdoCache cache, Phrase phrase, ISegment newSegment)
		{
			if (!String.IsNullOrEmpty(phrase.mediaFile))
			{
				if(!String.IsNullOrEmpty(phrase.speaker))
				{
					newSegment.SpeakerRA = FindOrCreateSpeaker(phrase.speaker, cache);
				}
				newSegment.BeginTimeOffset = phrase.beginTimeOffset;
				newSegment.EndTimeOffset = phrase.endTimeOffset;
				newSegment.MediaURIRA = cache.ServiceLocator.ObjectRepository.GetObject(new Guid(phrase.mediaFile)) as ICmMediaURI;
			}
		}

		private static ICmPerson FindOrCreateSpeaker(string speaker, FdoCache cache)
		{
			if(cache.LanguageProject.PeopleOA != null)
			{
				//find and return a person in this project whose name matches the speaker
				foreach (var person in cache.LanguageProject.PeopleOA.PossibilitiesOS)
				{
					if (person.Name.BestVernacularAnalysisAlternative.Text.Equals(speaker))
					{
						return (ICmPerson)person;
					}
				}
			}
			else
			{
				cache.LanguageProject.PeopleOA = cache.ServiceLocator.GetInstance<ICmPossibilityListFactory>().Create();
			}

			//person not found create one and add it.
			var newPerson = cache.ServiceLocator.GetInstance<ICmPersonFactory>().Create();
			cache.LanguageProject.PeopleOA.PossibilitiesOS.Add(newPerson);
			newPerson.Name.set_String(cache.DefaultVernWs, speaker);
			return newPerson;
		}

		private static void MergeWordToSegment(ISegment newSegment, Word word, ITsStrFactory tsStrFactory)
		{
			if(!String.IsNullOrEmpty(word.guid))
			{
				ICmObject repoObj;
				newSegment.Cache.ServiceLocator.ObjectRepository.TryGetObject(new Guid(word.guid), out repoObj);
				IAnalysis modelWord = repoObj as IAnalysis;
				if(modelWord != null)
				{
					newSegment.AnalysesRS.Add(modelWord);
				}
				else
				{
					AddWordToSegment(newSegment, word, tsStrFactory);
				}
			}
			else
			{
				AddWordToSegment(newSegment, word, tsStrFactory);
			}
		}

		private static bool CheckAndAddLanguagesInternal(FdoCache cache, Interlineartext interlinText, ILgWritingSystemFactory wsFactory, IThreadedProgress progress)
		{
			if (interlinText.languages != null)
			{
				foreach (var lang in interlinText.languages.language)
				{
					bool fIsVernacular;
					var writingSystem = SafelyGetWritingSystem(cache, wsFactory, lang, out fIsVernacular);
					if (fIsVernacular)
					{
						if (!cache.LanguageProject.CurrentVernacularWritingSystems.Contains(writingSystem.Handle))
						{
							//we need to invoke the dialog on the main thread so we can use the progress dialog as the parent.
							//otherwise the message box can be displayed behind everything
							IAsyncResult asyncResult = progress.ThreadHelper.BeginInvoke(
								new ShowDialogAboveProgressbarDelegate(ShowDialogAboveProgressbar),
								new object[]
								{
									progress,
									writingSystem.LanguageName + ITextStrings.ksImportVernacLangMissing,
									ITextStrings.ksImportVernacLangMissingTitle,
									MessageBoxButtons.OKCancel
								});
							var result = (DialogResult)progress.ThreadHelper.EndInvoke(asyncResult);
							if (result == DialogResult.OK)
							{
								cache.LanguageProject.AddToCurrentVernacularWritingSystems((IWritingSystem)writingSystem);
							}
							else
							{
								return false;
							}
						}
					}
					else
					{
						if (!cache.LanguageProject.CurrentAnalysisWritingSystems.Contains(writingSystem.Handle))
						{
							IAsyncResult asyncResult = progress.ThreadHelper.BeginInvoke(
								new ShowDialogAboveProgressbarDelegate(ShowDialogAboveProgressbar),
								new object[]
								{
									progress,
									writingSystem.LanguageName + ITextStrings.ksImportAnalysisLangMissing,
									ITextStrings.ksImportAnalysisLangMissingTitle,
									MessageBoxButtons.OKCancel
								});
							var result = (DialogResult)progress.ThreadHelper.EndInvoke(asyncResult);
							//alert the user
							if (result == DialogResult.OK)
							{
								//alert the user
								cache.LanguageProject.AddToCurrentAnalysisWritingSystems((IWritingSystem)writingSystem);
							}
							else
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		private static ILgWritingSystem SafelyGetWritingSystem(FdoCache cache, ILgWritingSystemFactory wsFactory,
			Language lang, out bool fIsVernacular)
		{
			fIsVernacular = false;
			if (lang.vernacularSpecified && lang.vernacular)
				fIsVernacular = true;
			ILgWritingSystem writingSystem = null;
			try
			{
				writingSystem = wsFactory.get_Engine(lang.lang);
			}
			catch (ArgumentException e)
			{
				IWritingSystem ws;
				WritingSystemServices.FindOrCreateSomeWritingSystem(cache, lang.lang,
					!fIsVernacular, fIsVernacular, out ws);
				writingSystem = ws;
				s_wsMapper.Add(lang.lang, writingSystem); // old id string -> new langWs mapping
			}
			return writingSystem;
		}

		private static void AddWordToSegment(ISegment newSegment, Word word, ITsStrFactory strFactory)
		{
			//use the items under the word to determine what kind of thing to add to the segment
			if (word.Items != null && word.Items.Length > 0)
			{
				var wsFact = newSegment.Cache.WritingSystemFactory;
				IAnalysis analysis = null;
				foreach (item wordItem in word.Items)
				{
					ITsString wordForm = null;
					switch (wordItem.type)
					{
						case "txt":
							wordForm = strFactory.MakeString(wordItem.Value,
								GetWsEngine(wsFact, wordItem.lang).Handle);
							analysis = WfiWordformServices.FindOrCreateWordform(newSegment.Cache, wordForm);
							break;
						case "punct":
							wordForm = strFactory.MakeString(wordItem.Value,
								GetWsEngine(wsFact, wordItem.lang).Handle);
							analysis = WfiWordformServices.FindOrCreatePunctuationform(newSegment.Cache, wordForm);
							break;
					}
				}
				if (analysis != null)
				{
					newSegment.AnalysesRS.Add(analysis);
				}
				else
				{
					Debug.Assert(analysis != null, "What else could this do?");
				}
				//Add any morphemes to the thing
				if (word.morphemes != null && word.morphemes.morphs.Length > 0)
				{
					//var bundle = newSegment.Cache.ServiceLocator.GetInstance<IWfiMorphBundleFactory>().Create();
					//analysis.Analysis.MorphBundlesOS.Add(bundle);
					//foreach (var morpheme in word.morphemes)
					//{
					//    //create a morpheme
					//    foreach(item item in morpheme.items)
					//    {
					//        //fill in morpheme's stuff
					//    }
					//}
				}
			}
		}

		private static void SetTextMetaAndMedia(FdoCache cache, Interlineartext interlinText, ILgWritingSystemFactory wsFactory, FDO.IText newText)
		{
			if (interlinText.Items != null) // apparently it is null if there are no items.
			{
				foreach (var item in interlinText.Items)
				{
					switch (item.type)
					{
						case "title":
							newText.Name.set_String(GetWsEngine(wsFactory, item.lang).Handle, item.Value);
							break;
						case "title-abbreviation":
							newText.Abbreviation.set_String(GetWsEngine(wsFactory, item.lang).Handle, item.Value);
							break;
						case "source":
							newText.Source.set_String(GetWsEngine(wsFactory, item.lang).Handle, item.Value);
							break;
						case "comment":
							newText.Description.set_String(GetWsEngine(wsFactory, item.lang).Handle, item.Value);
							break;
					}
				}
			}

			//handle media files section (ELAN initiated) needs to be processed before the paragraphs as segmenets could reference
			//these parts.
			if (interlinText.mediafiles != null)
			{
				var mediaFiles = newText.MediaFilesOA = cache.ServiceLocator.GetInstance<ICmMediaContainerFactory>().Create();
				mediaFiles.OffsetType = interlinText.mediafiles.offsetType;
				foreach (var mediaFile in interlinText.mediafiles.media)
				{
					var media = cache.ServiceLocator.GetInstance<ICmMediaURIFactory>().Create(cache, new Guid(mediaFile.guid));
					mediaFiles.MediaURIsOC.Add(media);
					media.MediaURI = mediaFile.location;
				}
			}
		}
	}
}
