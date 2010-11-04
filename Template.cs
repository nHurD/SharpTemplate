/******************************************************************
** File Name:		Template.cs 
** Creation Date: 	Saturday February 10, 2007 
** Original Author: Jonathan R. Steele (jrsteele@gmail.com)
** Description:		Implement iTemplate for generic templating
**					This is based loosely on the FastTemplate class
**					that was written in PHP(3/4/5)
******************************************************************/
using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Org.nHurD
{
	
	
	public class Template : iTemplate
	{
		
		private System.Collections.Generic.List<TagInfo> _tagCollection;
		
		protected string _templateDirectory;
		
		public Template() { 
			_templateDirectory = string.Empty;
			setupDefaultTags();
		}
		
		public Template(string p_dir) {
			_templateDirectory = p_dir;
			setupDefaultTags();
		}
		
		private void setupDefaultTags () {
			_tagCollection = new System.Collections.Generic.List<TagInfo>();
			
			/* output, master tag */
			TagInfo t = new TagInfo();
			t.tagName = "OUTPUT";
			t.tagValue = string.Empty;
			
			_tagCollection.Add(t);
		}
		
		/* Locate, and return the value for a given tag */
		private string _getTagValue ( string p_Tag, string p_format ) {
			
			int tagIdx = _getTagIndex( p_Tag );
			if (tagIdx != -1) {
				if (p_format == String.Empty)
					return _tagCollection[tagIdx].tagValue.ToString();
				else
					return String.Format("{0:" + p_format + "}", _tagCollection[tagIdx].tagValue);
			}
			
			return "";
			
		}
		
		
		/* Locate a tag with the given name */
		protected int _getTagIndex ( string p_tagName ) {
		
			/* default to not found */
			int _return = -1;
			
			for (int i=0; i < _tagCollection.Count; i++) {
				if (_tagCollection[i].tagName == p_tagName) _return = i;
			
			}
			
			return _return;
		}
		
		
		
		/* Do the dirty work */
		private string _parseTemplate( string p_tag ) {
			/* this will work recursively, 
				getting all the data for the child tags */
				
				
			string tag 			= String.Empty;
			string formatStr 	= String.Empty;
			
			
			/* Process any formatting strings */
			if (p_tag.IndexOf(":") > 0) {
				tag = p_tag.Substring(0,p_tag.IndexOf(":"));
				formatStr = p_tag.Replace(tag + ":","");
			} else
				tag = p_tag;
			
			/* The tag value with unparsed child tags */
			string topValue = _getTagValue( tag, formatStr );
			
			/* get the keys, and replace them with their values */
			foreach (string key in _findKeys( tag )) {
				topValue = topValue.Replace("{" + key + "}", _parseTemplate( key ));			
			}
				
			
			return topValue;
		
		}
		
		
		/* gather a list of keys to parse with the gien tag */
		private System.Collections.Generic.List<string> _findKeys( string p_tag ) {
		
			/* find the keys within the tag data for a tag */
			/* looking for everything between the curly braces { } */
			System.Collections.Generic.List<string> _return = new System.Collections.Generic.List<string>();
			
			
			System.Text.RegularExpressions.Regex reg =  new System.Text.RegularExpressions.Regex(@"{.*?}.*?");
				
				
				Match match = reg.Match(_getTagValue(p_tag, String.Empty));
				
				if (match.Length > 0) {
					
					while (match.Success) {
						_return.Add(match.Value.Replace("{","").Replace("}",""));
						match = match.NextMatch();
					}
					
					
				}
				
				return _return;
		}
		
		
		public string TemplateDirectory {
			get { return _templateDirectory; }
			set { _templateDirectory = value; }
		}
		
		/* Create and assign tags based on properties in the object */
		public void assignObject ( ref object p_object, string p_prefix ) {
			
			foreach (PropertyInfo property in p_object.GetType().GetProperties()) {
				TagInfo tmp = new TagInfo();
				object tmpValue = null;
				tmp.tagName = property.Name;
				
				/* Sanity Check. Ensure no null value */
				tmpValue = property.GetValue(p_object, null);
				if (tmpValue == null) tmpValue = String.Empty;
				
				tmp.tagValue = tmpValue;
				
				/* Add the tag to the collection */
				_tagCollection.Add(tmp);
			}
			
		}
		
		public void assignObject ( ref object p_object ) {
			assignObject ( ref p_object, "");
		}
		
		/* Individually assign a tagvalue */
		public void assignTag ( string p_tag, object p_value ) {
			TagInfo tmp = new TagInfo();
			int tagIdx = _getTagIndex(p_tag);
			
			tmp.tagName = p_tag;
			tmp.tagValue = p_value;
			
			if (tagIdx != -1)
				_tagCollection.RemoveAt(tagIdx);
				
			_tagCollection.Add(tmp);
					
		}
		
		public void parseTag ( string p_tag, string p_fileName ) {
		
			/* Make sure that the tag doesn't exist. if it does, replace it */
			int tagIdx = _getTagIndex(p_tag);
			
			
			TagInfo tmp = new TagInfo();
			tmp.tagName = p_tag;
			
			try {
				System.IO.StreamReader reader = new System.IO.StreamReader(
					this._templateDirectory + "/" + p_fileName);
				tmp.tagValue = reader.ReadToEnd();
				
				reader.Close();
			} catch {
				return;
			}
			
			if (tagIdx != -1)
				_tagCollection.RemoveAt(tagIdx);				


			_tagCollection.Add(tmp);

			return;
			
					
		}
		
		/* Go through a DataTable object and assign the values in the columns */
		public void parseDataTable ( 
			string p_rowTag, string p_rowTemplateFile, ref System.Data.DataTable p_table, string p_prefix) {
			
			string tagValue = "";
			string rowTemplate = "";
			TagInfo tmp = new TagInfo();
			tmp.tagName = p_rowTag;
			
			
			try { 
				System.IO.StreamReader reader = new System.IO.StreamReader(
					_templateDirectory + "/" + p_rowTemplateFile);
				rowTemplate = reader.ReadToEnd();
				reader.Close();
			} catch {
				return;
			}
			
			/* Add the tag to the collection, unparsed */
			tmp.tagValue = rowTemplate;
			_tagCollection.Add(tmp);
					
			
			
			foreach (DataRow row in p_table.Rows) {
				string tmpValue = rowTemplate;
				
				/* Handle string formatting */
				string tagName = String.Empty;
				string tagFormat = String.Empty;
				
				foreach (string tag in _findKeys(p_rowTag)) {
				
					if (tag.IndexOf(":") > 0) {
						tagName = tag.Substring(0,tag.IndexOf(":"));
						tagFormat = tag.Replace(tagName + ":","");
					} else {
						tagName = tag;
						tagFormat = String.Empty;
					}
					
					/* Sanity check */
					if (!p_table.Columns.Contains(tagName)) continue;
					
					string colValue = row[tagName].ToString();
					if (tagFormat != String.Empty) {
						colValue = String.Format("{0:" + tagFormat + "}", row[tagName]);
					}
					
					/* Replace the tag with the (formatted) value */
					tmpValue = tmpValue.Replace("{" + p_prefix + tag + "}", colValue);
					
						
					
				}
					
				/* append the new value */
				tagValue += tmpValue;
			}
			
			assignTag(p_rowTag, tagValue);

		}
		
		public void parseDataTable (
			string p_rowTag, string p_rowTemplateFile, ref System.Data.DataTable p_table ) {
			
			parseDataTable ( p_rowTag, p_rowTemplateFile, ref p_table, "" );
			
		}
		
		public string parseDataRow (  
			string p_rowTemplateFile,
			ref System.Data.DataColumnCollection p_cols,
			System.Data.DataRow p_row,
			string p_prefix) {
			
			string _return = "";
			
			try { 
				System.IO.StreamReader reader = new System.IO.StreamReader(
					_templateDirectory + "/" + p_rowTemplateFile);
				_return = reader.ReadToEnd();
				reader.Close();
			} catch {
				return String.Empty;
			}
			
			foreach (DataColumn col in p_cols) {
				_return = _return.Replace("{" + p_prefix + col.ColumnName + "}", p_row[col.ColumnName].ToString());			
			}
			
			return _return;
		}
			
		public string parseDataRow (  
			string p_rowTemplateFile,
			ref System.Data.DataColumnCollection p_cols,
			System.Data.DataRow p_row) {
			
			return parseDataRow (p_rowTemplateFile, ref p_cols, p_row, "");
			
		}
			
		
		/* Parse an array of objects */
		public void parseObjectCollection ( string p_rowTag,
			string p_rowTemplateFile,
			ref object[] p_objects,
			string p_prefix) {
			
			TagInfo tmp = new TagInfo();
			tmp.tagName = p_rowTag;
			
			string tagValue = String.Empty;
			string rowTemplate = String.Empty;
			
			/* open the row template file and store it in rowTemplate */
			try { 
				System.IO.StreamReader reader = new System.IO.StreamReader(
					_templateDirectory + "/" + p_rowTemplateFile);
				rowTemplate = reader.ReadToEnd();
				reader.Close();
			} catch {
				return;
			}
			
			tmp.tagName = p_rowTag;
			tmp.tagValue = rowTemplate;
			
			_tagCollection.Add(tmp);
			
			/* go through each of the objects and parse their properties */
			foreach (object obj in p_objects) {
				string tmpValue = rowTemplate;
				
				/* gather the keys from the template */
				foreach (string tag in _findKeys(p_rowTag)) {
					string tagName = tag;
					string tagFormat = String.Empty;
					
					/* Determine if any formatting needs to be applied */
					if (tag.IndexOf(":") > 0) {
						tagName = tag.Substring(0,tag.IndexOf(":"));
						tagFormat = tag.Replace(tagName + ":","");
					}
					
					/* Get the property */
					PropertyInfo property = obj.GetType().GetProperty(tagName);
					if (property == null) continue;
				
					/* Get the value of the property and perform a sanity check */
					object propertyValue = property.GetValue(obj,null);
					if (propertyValue == null) propertyValue = String.Empty;
					
					/* Apply any formatting strings */
					if (tagFormat != String.Empty)
						propertyValue = String.Format("{0:" + tagFormat + "}",propertyValue);
					else
						propertyValue = propertyValue.ToString();
						
					/* Set the value */
					tmpValue = tmpValue.Replace("{" + p_prefix + tag + "}",propertyValue.ToString());
															
					
				}
				
				tagValue += tmpValue;
			}
			
			/* Assign the tag to the template */
			assignTag(p_rowTag, tagValue);
		
			
		}
		
		public void parseObjectCollection ( string p_rowTag,
			string p_rowTemplateFile,
			ref object[] p_objects) {
			
			parseObjectCollection(p_rowTag, p_rowTemplateFile, ref p_objects, "");
			
		}
		
		
		/* output the template */
		public string printTemplate () {
			string _return = _parseTemplate ( "OUTPUT" );
			
			return _return;
						
		}		
		
		
		
	}
}
