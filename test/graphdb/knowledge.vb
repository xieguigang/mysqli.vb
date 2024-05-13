#Region "Microsoft.VisualBasic::5eee0257e0ccc24ef8c43f31e1851267, src\mysqli\test\graphdb\knowledge.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 215
    '    Code Lines: 88
    ' Comment Lines: 105
    '   Blank Lines: 22
    '     File Size: 10.42 KB


    ' Class knowledge
    ' 
    '     Properties: add_time, description, display_title, graph_size, id
    '                 key, node_type
    ' 
    '     Function: Clone, GetDeleteSQL, GetDumpInsertValue, (+2 Overloads) GetInsertSQL, (+2 Overloads) GetReplaceSQL
    '               GetUpdateSQL
    ' 
    ' 
    ' /********************************************************************************/

#End Region

REM  Oracle.LinuxCompatibility.MySQL.CodeSolution.VisualBasic.CodeGenerator
REM  MYSQL Schema Mapper
REM      for Microsoft VisualBasic.NET 2.1.0.2569

REM  Dump @11/22/2023 2:31:35 PM


Imports System.Data.Linq.Mapping
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports MySqlScript = Oracle.LinuxCompatibility.MySQL.Scripting.Extensions

Namespace mysql

''' <summary>
''' ```SQL
''' 
''' --
''' 
''' DROP TABLE IF EXISTS `knowledge`;
''' /*!40101 SET @saved_cs_client     = @@character_set_client */;
''' /*!50503 SET character_set_client = utf8mb4 */;
''' CREATE TABLE `knowledge` (
'''   `id` int unsigned NOT NULL COMMENT 'usually be the FN-1a hashcode of the ''key + node_type'' term',
'''   `key` varchar(255) NOT NULL COMMENT 'the unique key of current knowledge node data',
'''   `display_title` varchar(2048) NOT NULL COMMENT 'the display title text of current knowledge node data',
'''   `node_type` int unsigned NOT NULL COMMENT 'the node type enumeration number value, string value could be found in the knowledge vocabulary table',
'''   `graph_size` int unsigned NOT NULL DEFAULT '0' COMMENT 'the number of connected links to current knowledge node',
'''   `add_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'add time of current knowledge node data',
'''   `description` longtext NOT NULL COMMENT 'the description text about current knowledge data',
'''   PRIMARY KEY (`id`),
'''   UNIQUE KEY `id_UNIQUE` (`id`),
'''   KEY `key_index` (`key`) /*!80000 INVISIBLE */,
'''   KEY `type_index` (`node_type`)
''' ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COMMENT='knowlege data pool';
''' /*!40101 SET character_set_client = @saved_cs_client */;
''' 
''' --
''' ```
''' </summary>
''' <remarks></remarks>
<Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes.TableName("knowledge", Database:="graphql", SchemaSQL:="
CREATE TABLE `knowledge` (
  `id` int unsigned NOT NULL COMMENT 'usually be the FN-1a hashcode of the ''key + node_type'' term',
  `key` varchar(255) NOT NULL COMMENT 'the unique key of current knowledge node data',
  `display_title` varchar(2048) NOT NULL COMMENT 'the display title text of current knowledge node data',
  `node_type` int unsigned NOT NULL COMMENT 'the node type enumeration number value, string value could be found in the knowledge vocabulary table',
  `graph_size` int unsigned NOT NULL DEFAULT '0' COMMENT 'the number of connected links to current knowledge node',
  `add_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'add time of current knowledge node data',
  `description` longtext NOT NULL COMMENT 'the description text about current knowledge data',
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `key_index` (`key`) /*!80000 INVISIBLE */,
  KEY `type_index` (`node_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COMMENT='knowlege data pool';")>
Public Class knowledge: Inherits Oracle.LinuxCompatibility.MySQL.MySQLTable
#Region "Public Property Mapping To Database Fields"
''' <summary>
''' usually be the FN-1a hashcode of the ''key + node_type'' term
''' </summary>
''' <value></value>
''' <returns></returns>
''' <remarks></remarks>
    <DatabaseField("id"), PrimaryKey, NotNull, DataType(MySqlDbType.UInt32, "11"), Column(Name:="id"), XmlAttribute> Public Property id As UInteger
''' <summary>
''' the unique key of current knowledge node data
''' </summary>
''' <value></value>
''' <returns></returns>
''' <remarks></remarks>
    <DatabaseField("key"), NotNull, DataType(MySqlDbType.VarChar, "255"), Column(Name:="key")> Public Property key As String
''' <summary>
''' the display title text of current knowledge node data
''' </summary>
''' <value></value>
''' <returns></returns>
''' <remarks></remarks>
    <DatabaseField("display_title"), NotNull, DataType(MySqlDbType.VarChar, "2048"), Column(Name:="display_title")> Public Property display_title As String
''' <summary>
''' the node type enumeration number value, string value could be found in the knowledge vocabulary table
''' </summary>
''' <value></value>
''' <returns></returns>
''' <remarks></remarks>
    <DatabaseField("node_type"), NotNull, DataType(MySqlDbType.UInt32, "11"), Column(Name:="node_type")> Public Property node_type As UInteger
''' <summary>
''' the number of connected links to current knowledge node
''' </summary>
''' <value></value>
''' <returns></returns>
''' <remarks></remarks>
    <DatabaseField("graph_size"), NotNull, DataType(MySqlDbType.UInt32, "11"), Column(Name:="graph_size")> Public Property graph_size As UInteger
''' <summary>
''' add time of current knowledge node data
''' </summary>
''' <value></value>
''' <returns></returns>
''' <remarks></remarks>
    <DatabaseField("add_time"), NotNull, DataType(MySqlDbType.DateTime), Column(Name:="add_time")> Public Property add_time As Date
''' <summary>
''' the description text about current knowledge data
''' </summary>
''' <value></value>
''' <returns></returns>
''' <remarks></remarks>
    <DatabaseField("description"), NotNull, DataType(MySqlDbType.Text), Column(Name:="description")> Public Property description As String
#End Region
#Region "Public SQL Interface"
#Region "Interface SQL"
    Friend Shared ReadOnly INSERT_SQL$ = 
        <SQL>INSERT INTO `knowledge` (`id`, `key`, `display_title`, `node_type`, `graph_size`, `add_time`, `description`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');</SQL>

    Friend Shared ReadOnly INSERT_AI_SQL$ = 
        <SQL>INSERT INTO `knowledge` (`id`, `key`, `display_title`, `node_type`, `graph_size`, `add_time`, `description`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');</SQL>

    Friend Shared ReadOnly REPLACE_SQL$ = 
        <SQL>REPLACE INTO `knowledge` (`id`, `key`, `display_title`, `node_type`, `graph_size`, `add_time`, `description`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');</SQL>

    Friend Shared ReadOnly REPLACE_AI_SQL$ = 
        <SQL>REPLACE INTO `knowledge` (`id`, `key`, `display_title`, `node_type`, `graph_size`, `add_time`, `description`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');</SQL>

    Friend Shared ReadOnly DELETE_SQL$ =
        <SQL>DELETE FROM `knowledge` WHERE `id` = '{0}';</SQL>

    Friend Shared ReadOnly UPDATE_SQL$ = 
        <SQL>UPDATE `knowledge` SET `id`='{0}', `key`='{1}', `display_title`='{2}', `node_type`='{3}', `graph_size`='{4}', `add_time`='{5}', `description`='{6}' WHERE `id` = '{7}';</SQL>

#End Region

''' <summary>
''' ```SQL
''' DELETE FROM `knowledge` WHERE `id` = '{0}';
''' ```
''' </summary>
    Public Overrides Function GetDeleteSQL() As String
        Return String.Format(DELETE_SQL, id)
    End Function

''' <summary>
''' ```SQL
''' INSERT INTO `knowledge` (`id`, `key`, `display_title`, `node_type`, `graph_size`, `add_time`, `description`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');
''' ```
''' </summary>
    Public Overrides Function GetInsertSQL() As String
        Return String.Format(INSERT_SQL, id, key, display_title, node_type, graph_size, MySqlScript.ToMySqlDateTimeString(add_time), description)
    End Function

''' <summary>
''' ```SQL
''' INSERT INTO `knowledge` (`id`, `key`, `display_title`, `node_type`, `graph_size`, `add_time`, `description`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');
''' ```
''' </summary>
    Public Overrides Function GetInsertSQL(AI As Boolean) As String
        If AI Then
        Return String.Format(INSERT_AI_SQL, id, key, display_title, node_type, graph_size, MySqlScript.ToMySqlDateTimeString(add_time), description)
        Else
        Return String.Format(INSERT_SQL, id, key, display_title, node_type, graph_size, MySqlScript.ToMySqlDateTimeString(add_time), description)
        End If
    End Function

''' <summary>
''' <see cref="GetInsertSQL"/>
''' </summary>
    Public Overrides Function GetDumpInsertValue(AI As Boolean) As String
        If AI Then
            Return $"('{id}', '{key}', '{display_title}', '{node_type}', '{graph_size}', '{add_time.ToString("yyyy-MM-dd hh:mm:ss")}', '{description}')"
        Else
            Return $"('{id}', '{key}', '{display_title}', '{node_type}', '{graph_size}', '{add_time.ToString("yyyy-MM-dd hh:mm:ss")}', '{description}')"
        End If
    End Function


''' <summary>
''' ```SQL
''' REPLACE INTO `knowledge` (`id`, `key`, `display_title`, `node_type`, `graph_size`, `add_time`, `description`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');
''' ```
''' </summary>
    Public Overrides Function GetReplaceSQL() As String
        Return String.Format(REPLACE_SQL, id, key, display_title, node_type, graph_size, MySqlScript.ToMySqlDateTimeString(add_time), description)
    End Function

''' <summary>
''' ```SQL
''' REPLACE INTO `knowledge` (`id`, `key`, `display_title`, `node_type`, `graph_size`, `add_time`, `description`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');
''' ```
''' </summary>
    Public Overrides Function GetReplaceSQL(AI As Boolean) As String
        If AI Then
        Return String.Format(REPLACE_AI_SQL, id, key, display_title, node_type, graph_size, MySqlScript.ToMySqlDateTimeString(add_time), description)
        Else
        Return String.Format(REPLACE_SQL, id, key, display_title, node_type, graph_size, MySqlScript.ToMySqlDateTimeString(add_time), description)
        End If
    End Function

''' <summary>
''' ```SQL
''' UPDATE `knowledge` SET `id`='{0}', `key`='{1}', `display_title`='{2}', `node_type`='{3}', `graph_size`='{4}', `add_time`='{5}', `description`='{6}' WHERE `id` = '{7}';
''' ```
''' </summary>
    Public Overrides Function GetUpdateSQL() As String
        Return String.Format(UPDATE_SQL, id, key, display_title, node_type, graph_size, MySqlScript.ToMySqlDateTimeString(add_time), description, id)
    End Function
#End Region

''' <summary>
                     ''' Memberwise clone of current table Object.
                     ''' </summary>
                     Public Function Clone() As knowledge
                         Return DirectCast(MyClass.MemberwiseClone, knowledge)
                     End Function
End Class


End Namespace
