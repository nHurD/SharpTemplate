/******************************************************************
** File Name:		iTemplate.cs 
** Creation Date: 	Saturday February 10, 2007 
** Original Author: Jonathan R. Steele (jrsteele@gmail.com)
** Description:		Base implementation for templating
******************************************************************/
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SharpTemplate
{
	
	
	public interface iTemplate
	{
	
		/* Some basic properties */
		string TemplateDirectory { get; set; }
		
	
		/* Assign template variables based on the properties 
		 * found within the object
		*/
		void assignObject ( ref object p_object );
		void assignObject ( ref object p_object, string p_prefix );
		
		
		/* Single tag assignment */
		void assignTag ( string p_tag, object p_value );
		
		/* Parse a file and assign it to a tag */
		void parseTag ( string p_tag, string p_fileName );
		
		
		/* Parse a DataTable Object */
		void parseDataTable ( string p_rowTag, 
			string p_rowTemplateFile, 
			ref System.Data.DataTable p_table, 
			string p_prefix);
			
		void parseDataTable ( string p_rowTag, 
			string p_rowTemplateFile, 
			ref System.Data.DataTable p_table );
			
		/* Parase an individual DataRow object */
		string parseDataRow (  
			string p_rowTemplateFile,
			ref System.Data.DataColumnCollection p_cols,
			System.Data.DataRow p_row,
			string p_prefix);
			
		string parseDataRow (  
			string p_rowTemplateFile,
			ref System.Data.DataColumnCollection p_cols,
			System.Data.DataRow p_row);
		
			
		/* Parse an array of objects */
		void parseObjectCollection ( string p_rowTag,
			string p_rowTemplateFile,
			ref object[] p_objects,
			string p_prefix);
		
		void parseObjectCollection ( string p_rowTag,
			string p_rowTemplateFile,
			ref object[] p_objects);
			
		
		
		/* Put the final template into a string */
		string printTemplate ();
		
	}
}
