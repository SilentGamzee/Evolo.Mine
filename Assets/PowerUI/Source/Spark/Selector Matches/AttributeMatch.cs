//--------------------------------------//               PowerUI////        For documentation or //    if you have any issues, visit//        powerUI.kulestar.com////    Copyright � 2013 Kulestar Ltd//          www.kulestar.com//--------------------------------------using System;namespace Css{		/// <summary>	/// Describes if an element has an attribute which matches.	/// <summary>	public class AttributeMatch:LocalMatcher{				public string Value;		public int MatchMode;		public string Attribute;		public bool CaseInsensitive;						public override void AddToDocument(ReflowDocument document,StyleSheet sheet){						if(sheet!=null && document!=null){								// Add attribute to stylesheet:				int count;				document.SelectorAttributes.TryGetValue(Attribute,out count);				document.SelectorAttributes[Attribute]=count++;							}					}				public override void RemoveFromDocument(ReflowDocument document,StyleSheet sheet){						if(sheet!=null && document!=null){				// Remove attribute from stylesheet:				int count;								if(document.SelectorAttributes.TryGetValue(Attribute,out count)){					count--;										if(count<=0){						document.SelectorAttributes.Remove(Attribute);					}else{						document.SelectorAttributes[Attribute]=count;					}									}							}					}				/// <summary>Loads this attribute match from the given value.</summary>		public void Load(ValueSet set){						Attribute=set[0].Text;						int count=set.Count;						if(count==1){				return;			}						// Get the text:			string text=set[1].Text;						switch(text){				default:				case "=":					MatchMode=0;				break;				case "^=":					MatchMode=1;				break;				case "$=":					MatchMode=2;				break;				case "*=":					MatchMode=3;				break;				case "~=":					MatchMode=4;				break;				case "|=":					MatchMode=5;				break;			}						// Get the value:			Value=set[2].Text;						// Look out for 'i' (case insensitive)			if(count>3){								if(set[3].Text=="i"){										CaseInsensitive=true;					Value=Value.ToLower();									}							}					}				public override bool TryMatch(Dom.Node context){						if(context==null){				return false;			}						// Read it:			string value=context.getAttribute(Attribute);						if(value==null){				return false;			}						if(Value==null){				// Attribute must just exist.				return true;			}						if(CaseInsensitive){				value=value.ToLower();			}						switch(MatchMode){				default:				case 0:					// Equals: (=)					return (value==Value);				case 1:					// Starts with: (^=)					return value.StartsWith(Value);				case 2:					// Ends with: ($=)					return value.EndsWith(Value);				case 3:					// Contains substring: (*=)					return value.Contains(Value);				case 4:					// Contains word: (~=)					return ((value==Value) || value.Contains(" "+Value+" ") || value.StartsWith(Value+" ") || value.EndsWith(" "+Value) );				case 5:					// Starts with value and - (|=)					return ((value==Value) || value.StartsWith(Value+"-") );			}					}				public override bool Equals(LocalMatcher match){						AttributeMatch atMatch=match as AttributeMatch;						return (atMatch!=null && atMatch.Attribute==Attribute && atMatch.MatchMode==MatchMode && atMatch.Value==Value);					}				public override string ToString(){			return "["+Attribute+"=\""+Value+"\"]";		}			}	}