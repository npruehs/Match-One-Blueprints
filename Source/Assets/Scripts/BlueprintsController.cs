using System.Collections.Generic;
using System.IO;
using System.Xml;

using Entitas.Blueprints;
using Entitas.Blueprints.Xml;

using UnityEngine;

public class BlueprintsController : MonoBehaviour
{
    #region Fields

    private readonly Dictionary<string, Blueprint> blueprints = new Dictionary<string, Blueprint>();

    public TextAsset BlueprintsFile;

    #endregion

    #region Public Methods and Operators

    /// <summary>
    ///   Gets the blueprint with the specified id, or <c>null</c> if none could be found.
    /// </summary>
    /// <param name="blueprintId">Id of the blueprint to get.</param>
    /// <returns>Blueprint with the specified id, or <c>null</c> if none could be found.</returns>
    public Blueprint GetBlueprint(string blueprintId)
    {
        Blueprint blueprint;
        this.blueprints.TryGetValue(blueprintId, out blueprint);
        return blueprint;
    }

    #endregion

    #region Methods

    [ContextMenu("Load Blueprints")]
    private void LoadBlueprints()
    {
        if (this.BlueprintsFile == null)
        {
            Debug.LogError("Blueprints file missing.", this);
            return;
        }

        var blueprintFileText = this.BlueprintsFile.text;

        using (TextReader textReader = new StringReader(blueprintFileText))
        {
            var xmlReaderSettings = new XmlReaderSettings { IgnoreWhitespace = true };

            using (XmlReader xmlReader = XmlReader.Create(textReader, xmlReaderSettings))
            {
                var blueprintXmlSerializer = new BlueprintXmlSerializer();
                var deserializedBlueprints = blueprintXmlSerializer.ReadBlueprints(xmlReader);

                foreach (var blueprint in deserializedBlueprints)
                {
                    this.blueprints.Add(blueprint.Id, blueprint);
                    Debug.Log(string.Format("Added blueprint {0}.", blueprint.Id), this);
                }
            }
        }
    }

    private void Awake()
    {
        this.LoadBlueprints();
    }

    #endregion
}