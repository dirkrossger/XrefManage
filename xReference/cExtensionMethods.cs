using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Windows.Forms;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

namespace xReference
{
    public struct Test
    {
        public string Pnr { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
        public string Code { get; set; }
    }

    /// <summary>
    /// Contains extension methods that facilitate working with objects in the context of a transaction.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Executes a delegate function within the context of a transaction on the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="action">A delegate that takes the <b>Transaction</b> as an argument.</param>
        public static void UsingTransaction(this Database database, Action<Transaction> action)
        {
            using (var tr = database.TransactionManager.StartTransaction())
            {
                try
                {
                    action(tr);
                    tr.Commit();
                }
                catch (Exception)
                {
                    tr.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Executes a delegate function within the context of a transaction on the specified document.
        /// The document is locked before the transaction starts.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="action">A delegate that takes the <b>Transaction</b> as an argument.</param>
        public static void UsingTransaction(this Document document, Action<Transaction> action)
        {
            using (document.LockDocument())
            {
                document.Database.UsingTransaction(action);
            }
        }

        /// <summary>
        /// Opens a database-resident object as the specified type within the context of the specified transaction,
        /// using the specified open mode.
        /// </summary>
        /// <typeparam name="T">The type of object that the objectId represents.</typeparam>
        /// <param name="objectId">The object id.</param>
        /// <param name="tr">The transaction.</param>
        /// <param name="openMode">The open mode.</param>
        /// <returns>The database-resident object.</returns>
        public static T OpenAs<T>(this ObjectId objectId, Transaction tr, OpenMode openMode = OpenMode.ForRead)
            where T : DBObject
        {
            return (T)tr.GetObject(objectId, openMode);
        }

        /// <summary>
        /// Opens a database-resident object as the specified type within the context of the specified transaction,
        /// using the specified open mode.
        /// </summary>
        /// <typeparam name="T">The type of object that the objectId represents.</typeparam>
        /// <param name="objectId">The object id.</param>
        /// <returns>The database-resident object.</returns>
        public static T OpenAs<T>(this ObjectId objectId)
            where T : DBObject
        {
            return (T)objectId.GetObject(OpenMode.ForRead);
        }

        /// <summary>
        /// Upgrades the open mode of the specified object to ForWrite.
        /// </summary>
        /// <typeparam name="T">The type of DBObject.</typeparam>
        /// <param name="obj">The DBObject instance.</param>
        /// <returns>The original instance.</returns>
        public static T ForWrite<T>(this T obj) where T : DBObject
        {
            if (!obj.IsWriteEnabled)
                obj.UpgradeOpen();
            return obj;
        }

        /// <summary>
        /// Executes a delegate function in the context of a transaction, and passes it the collection
        /// of ObjectIds for the specified block table record.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="blockName">Name of the block.</param>
        /// <param name="action">A delegate that takes the transaction and the ObjectIds as arguments.</param>
        public static void UsingBlockTable(this Database database, string blockName, Action<Transaction, IEnumerable<ObjectId>> action)
        {
            database.UsingTransaction(
                tr =>
                {
                    // Get the block table
                    var blockTable = database.BlockTableId.OpenAs<BlockTable>(tr);

                    // Get the block table record
                    var tableRecord = blockTable[blockName].OpenAs<BlockTableRecord>(tr);

                    // Invoke the method
                    action(tr, tableRecord.Cast<ObjectId>());
                });
        }

        /// <summary>
        /// Executes a delegate function in the context of a transaction, and passes it the collection
        /// of Entity objects for the specified block table record.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="blockName">Name of the block.</param>
        /// <param name="action">A delegate that takes the transaction and the Entity collection as arguments.</param>
        public static void UsingBlockTable(this Database database, string blockName, Action<IEnumerable<Entity>> action)
        {
            database.UsingBlockTable
                (blockName,
                    (tr, blockTable) => action(from id in blockTable select id.OpenAs<Entity>(tr)));
        }

        /// <summary>
        /// Executes a delegate function in the context of a transaction, and passes it the collection
        /// of objects from model space of the specified type.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="action">A delegate that takes the transaction and the Entity collection as arguments.</param>
        /// <typeparamref name="T">The type of object to retrieve.</typeparamref>
        public static void UsingModelSpace<T>(this Database database, Action<IEnumerable<T>> action) where T : Entity
        {
            database.UsingModelSpace(
                (tr, ms) =>
                {
                    var rxClass = RXObject.GetClass(typeof(T));

                    action(from id in ms
                           where id.ObjectClass.IsDerivedFrom(rxClass)
                           select id.OpenAs<T>(tr));
                });
        }

        /// <summary>
        /// Executes a delegate function in the context of a transaction, and passes it the collection
        /// of ObjectIds for the model space block table record.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="action">A delegate that takes the transaction and the ObjectIds as arguments.</param>
        public static void UsingModelSpace(this Database database, Action<Transaction, IEnumerable<ObjectId>> action)
        {
            database.UsingBlockTable(BlockTableRecord.ModelSpace, action);
        }

        /// <summary>
        /// Executes a delegate function in the context of a transaction, and passes it the model space
        /// block table record.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="action">A delegate that takes the transaction and the model space block table record as arguments.</param>
        public static void UsingModelSpace(this Database database, Action<Transaction, BlockTableRecord> action)
        {
            database.UsingTransaction(
                tr =>
                {
                    // Get the block table
                    var blockTable = database.BlockTableId.OpenAs<BlockTable>(tr);

                    // Get the block table record
                    var tableRecord = blockTable[BlockTableRecord.ModelSpace].OpenAs<BlockTableRecord>(tr, OpenMode.ForWrite);

                    // Invoke the method
                    action(tr, tableRecord);
                });
        }

        /// <summary>
        /// Creates a new entity in the specified block table record.
        /// </summary>
        /// <typeparam name="T">The type of entity to create.</typeparam>
        /// <param name="blockTableRecord">The block table record.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="action">A delegate that is called with the newly created entity, just before it is added to the database.</param>
        /// <returns>The <b>ObjectId</b> of the newly created entity.</returns>
        public static ObjectId Create<T>(this BlockTableRecord blockTableRecord, Transaction transaction, Action<T> action)
            where T : Entity, new()
        {
            var obj = new T();
            obj.SetDatabaseDefaults();
            action(obj);
            var objectId = blockTableRecord.AppendEntity(obj);
            transaction.AddNewlyCreatedDBObject(obj, true);
            return objectId;
        }

        /// <summary>
        /// Creates a new entity of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of entity to create.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="action">A delegate that is called with the newly created entity, just before it is added to the database.</param>
        /// <returns>The <b>ObjectId</b> of the newly created entity.</returns>
        public static ObjectId Create<T>(this Database database, Action<T> action)
            where T : Entity, new()
        {
            var objectId = ObjectId.Null;

            database.UsingModelSpace(
                (tr, modelSpace) => { objectId = modelSpace.Create(tr, action); });

            return objectId;
        }

        /// <summary>
        /// Creates a layer using the specified name and color.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="layerName">Name of the layer.</param>
        /// <param name="color">The color.</param>
        /// <returns>The <b>ObjectId</b> of the newly created layer.</returns>
        public static ObjectId CreateLayer(this Database database, string layerName, Color color = null)
        {
            var objectId = ObjectId.Null;
            database.UsingTransaction(
                tr =>
                {
                    var layerTable = database.LayerTableId.OpenAs<LayerTable>().ForWrite();

                    var layer = new LayerTableRecord { Name = layerName };
                    if (color != null)
                        layer.Color = color;
                    objectId = layerTable.Add(layer);
                    tr.AddNewlyCreatedDBObject(layer, true);
                });
            return objectId;
        }

        /// <summary>
        /// Extension method that allows you to iterate through model space and perform an action
        /// on a specific type of object.
        /// </summary>
        /// <typeparam name="T">The type of object to search for.</typeparam>
        /// <param name="database">The database to use.</param>
        /// <param name="action">A delegate that is called for each object found of the specified type.</param>
        public static void ForEach<T>(this Database database, Action<T> action)
            where T : Entity
        {
            database.UsingModelSpace((tr, modelSpace) => modelSpace.ForEach(tr, action));
        }

        /// <summary>
        /// Extension method that allows you to iterate through model space and perform an action
        /// on a specific type of object.
        /// </summary>
        /// <typeparam name="T">The type of object to search for.</typeparam>
        /// <param name="database">The database to use.</param>
        /// <param name="predicate"></param>
        /// <param name="action">A delegate that is called for each object found of the specified type.</param>
        public static void ForEach<T>(this Database database, Func<T, bool> predicate, Action<T> action)
            where T : Entity
        {
            database.ForEach<T>(
                obj =>
                {
                    if (predicate(obj))
                        action(obj);
                });
        }

        /// <summary>
        /// Iterates through the specified symbol table, and performs an action on each symbol table record.
        /// </summary>
        /// <typeparam name="T">The type of symbol table record.</typeparam>
        /// <param name="tableId">The table id.</param>
        /// <param name="action">A delegate that is called for each record.</param>
        public static void ForEach<T>(this ObjectId tableId, Action<T> action) where T : SymbolTableRecord
        {
            tableId.Database.UsingTransaction(tr => tableId.OpenAs<SymbolTable>(tr).Cast<ObjectId>().ForEach(tr, action));
        }

        /// <summary>
        /// Extension method that allows you to iterate through the objects in a block table
        /// record and perform an action on a specific type of object.
        /// </summary>
        /// <typeparam name="T">The type of object to search for.</typeparam>
        /// <param name="btr">The block table record to iterate.</param>
        /// <param name="tr">The active transaction.</param>
        /// <param name="action">A delegate that is called for each object found of the specified type.</param>
        public static void ForEach<T>(this IEnumerable<ObjectId> btr, Transaction tr, Action<T> action)
            where T : DBObject
        {
            var theClass = RXObject.GetClass(typeof(T));

            // Loop through the entities in model space
            foreach (ObjectId objectId in btr)
            {
                // Look for entities of the correct type
                if (objectId.ObjectClass.IsDerivedFrom(theClass))
                {
                    action(objectId.OpenAs<T>(tr));
                }
            }
        }

        /// <summary>
        /// Extension method that allows you to iterate through model space and perform an action
        /// on a specific type of object.
        /// </summary>
        /// <typeparam name="T">The type of object to search for.</typeparam>
        /// <param name="document">The document to use.</param>
        /// <param name="action">A delegate that is called for each object found of the specified type.</param>
        /// <remarks>This method locks the specified document.</remarks>
        public static void ForEach<T>(this Document document, Action<T> action)
            where T : Entity
        {
            using (document.LockDocument())
            {
                document.Database.ForEach(action);
            }
        }

        /// <summary>
        /// Executes a delegate function with the collection of layers in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="action">A delegate that takes the collection of layers as an argument.</param>
        public static void UsingLayerTable(this Database database, Action<IEnumerable<LayerTableRecord>> action)
        {
            database.UsingTransaction(
                tr => action(from ObjectId id in database.LayerTableId.OpenAs<LayerTable>(tr)
                             select id.OpenAs<LayerTableRecord>(tr)));
        }

        /// <summary>
        /// Locks the document, opens the specified object, and passes it to the specified delegate.
        /// </summary>
        /// <typeparam name="T">The type of object the objectId represents.</typeparam>
        /// <param name="document">The document.</param>
        /// <param name="objectId">The object id.</param>
        /// <param name="openMode">The open mode.</param>
        /// <param name="action">A delegate that takes the opened object as an argument.</param>
        public static void OpenAs<T>(this Document document, ObjectId objectId, OpenMode openMode, Action<T> action)
            where T : DBObject
        {
            document.UsingTransaction(tr => action(objectId.OpenAs<T>(tr, openMode)));
        }

        /// <summary>
        /// Used to get a single value from a database-resident object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="objectId">The object id.</param>
        /// <param name="func">A delegate that takes the object as an argument and returns the value.</param>
        /// <returns>A value of the specified type.</returns>
        public static TResult GetValue<TObject, TResult>(this ObjectId objectId, Func<TObject, TResult> func)
            where TObject : DBObject
        {
            var result = default(TResult);

            objectId.Database.UsingTransaction(
                tr => { result = func(objectId.OpenAs<TObject>(tr)); });

            return result;
        }

        /// <summary>
        /// Opens the database object with the specified object ID and open mode, and calls a delegate
        /// with the opened object.
        /// </summary>
        /// <typeparam name="T">The type of <b>DBObject</b> being opened.</typeparam>
        /// <param name="objectId">The object id.</param>
        /// <param name="openMode">The open mode.</param>
        /// <param name="action">A delegate that will receive the opened object.</param>
        public static void OpenAs<T>(this ObjectId objectId, OpenMode openMode, Action<T> action)
            where T : DBObject
        {
            objectId.Database.UsingTransaction(tr => action(objectId.OpenAs<T>(tr, openMode)));
        }

        /// <summary>
        /// Opens the database object with the specified object ID for write, and calls a delegate
        /// with the opened object.
        /// </summary>
        /// <typeparam name="T">The type of <b>DBObject</b> being opened.</typeparam>
        /// <param name="objectId">The object id.</param>
        /// <param name="action">A delegate that will receive the opened object.</param>
        public static void OpenForWriteAs<T>(this ObjectId objectId, Action<T> action)
            where T : DBObject
        {
            objectId.Database.UsingTransaction(tr => action(objectId.OpenAs<T>(tr, OpenMode.ForWrite)));
        }

        /// <summary>
        /// Gets an <b>IEnumerable</b> for the layer table of the specified database, within the context
        /// of the specified transaction.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="tr">The transaction.</param>
        /// <param name="openMode">The open mode.</param>
        /// <returns>An <b>IEnumerable</b> for the layer table.</returns>
        //public static IEnumerable<LayerTableRecord> Layers(this Database database, Transaction tr,
        //    OpenMode openMode = OpenMode.ForRead)
        //{
        //    return database.LayerTableId
        //        .OpenAs<LayerTable>(tr)
        //        .Cast<ObjectId>()
        //        .OfType<LayerTableRecord>(tr, openMode);
        //}

        /// <summary>
        /// Gets an <b>IEnumerable</b> for the layer table of the specified database, within the context
        /// of the specified transaction.
        /// </summary>
        /// <remarks>
        /// This method is experimental, and not safe to use. If an exception is thrown by the caller within
        /// the loop, the finally block will not be called.
        /// </remarks>
        /// <param name="database">The database.</param>
        /// <param name="tr">The transaction.</param>
        /// <param name="openMode">The open mode.</param>
        /// <returns>An <b>IEnumerable</b> for the layer table.</returns>
        public static IEnumerable<LayerTableRecord> Layers(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            Active.WriteMessage("Creating transaction.\n");
            var tr = database.TransactionManager.StartTransaction();
            try
            {
                var layerTable = database.LayerTableId.OpenAs<LayerTable>();
                Active.WriteMessage("Start enumerating layers.\n");

                foreach (var id in layerTable)
                    yield return id.OpenAs<LayerTableRecord>(tr, openMode);
            }
            finally
            {
                Active.WriteMessage("Disposing transaction.\n");
                tr.Commit();
                tr.Dispose();
            }
        }

        /// <summary>
        /// Prompts the user to select an entity of the specified type, and calls a delegate with the
        /// open entity object.
        /// </summary>
        /// <typeparam name="T">The type of object being selected.</typeparam>
        /// <param name="editor">The editor.</param>
        /// <param name="message">The prompt message.</param>
        /// <param name="openMode">The open mode.</param>
        /// <param name="action">A delegate that takes the open object as an argument.</param>
        public static void GetEntity<T>(this Editor editor, string message, OpenMode openMode, Action<T> action) where T : Entity
        {
            var rxClass = RXObject.GetClass(typeof(T));

            var options = new PromptEntityOptions(message);
            options.SetRejectMessage(string.Format("Selected object is not an {0}", rxClass.Name));
            options.AddAllowedClass(typeof(T), false);

            var result = Active.Editor.GetEntity(options);

            if (result.Status == PromptStatus.OK)
                result.ObjectId.OpenAs(openMode, action);
        }

    }
}