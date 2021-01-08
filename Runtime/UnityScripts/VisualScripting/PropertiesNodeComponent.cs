﻿using ScriptingLanguage.VisualScripting.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class PropertiesNodeComponent : NodeComponent
    {
        [Serializable]
        [NodeTarget(typeof(PropertiesNodeComponent))]
        public class PropertiesNode : INode
        {
            public IEnumerable<Endpoint> Endpoints
            {
                get
                {
                    yield return ObjectEndpoint;
                    yield return ResultEndpoint;
                    yield return TypeNameEndpoint;
                }
            }
            public Endpoint ObjectEndpoint = new Endpoint();
            public Endpoint ResultEndpoint = new Endpoint();
            public Endpoint TypeNameEndpoint = new Endpoint();
            public List<string> Properties = new List<string>();

            public string GenerateCode(Endpoint endpoint, NodesDB nodesDB, CodeGenerationContext context)
            {
                if (endpoint == TypeNameEndpoint) {
                    var str = "";
                    foreach (var p in Properties) {
                        str += $".{p}";
                    }
                    if (str.Length > 0) {
                        str = str.Substring(1);
                    }
                    return $"\"{str}\"";
                }

                if (endpoint == ResultEndpoint) {

                    string str = "";
                    foreach (var prop in Properties)
                    {
                        str += $".{prop}";
                    }

                    var objectEndpoint = ObjectEndpoint.LinkedEndpoints.FirstOrDefault();
                    if (objectEndpoint == null)
                    {
                        str = str.Substring(1);
                        return str;
                    }

                    var objectEndpointNode = nodesDB.GetNodeByEndpoint(objectEndpoint);
                    string code;
                    using (context.CreateTemporaryCustomContext(null))
                    {
                        code = objectEndpointNode.GenerateCode(objectEndpoint, nodesDB, context);
                    }

                    return $"{code}{str}";
                }
                throw new InvalidOperationException();
            }
        }
        public EndpointComponent ObjectEndpoint;
        public EndpointComponent ResultEndpoint;
        public EndpointComponent TypeNameEndpoint;
        public Button PropertyTemplate;
        public RectTransform PropertiesRoot;

        public PropertiesNode _node;

        public override INode NodeTemplate => new PropertiesNode();

        public override INode Node => _node;

        public override void ParticularInit(INode node)
        {
            _node = node as PropertiesNode;

            ObjectEndpoint.Endpoint = _node.ObjectEndpoint;
            ResultEndpoint.Endpoint = _node.ResultEndpoint;
            TypeNameEndpoint.Endpoint = _node.TypeNameEndpoint;
            foreach (var prop in _node.Properties) {
                AddPropertyTemplate(prop);
            }
        }

        public void AddPropertyTemplate() 
        {
            AddPropertyTemplate(null);
        }

        private void AddPropertyTemplate(string propertyName) 
        {
            var parentTransform = PropertiesRoot;
            var property = Instantiate(PropertyTemplate, parentTransform);
            property.gameObject.SetActive(true);
            var text = property.GetComponent<Text>();

            var propertyExplorer = property.GetComponentInChildren<PropertyExplorer>();
            propertyExplorer.gameObject.SetActive(false);

            var inputField = propertyExplorer.GetComponentInChildren<InputField>(true);
            var okButton = propertyExplorer.GetComponentInChildren<Button>();

            if (string.IsNullOrWhiteSpace(propertyName)) {
                _node.Properties.Add(text.text);
            } else {
                text.text = propertyName;
            }

            okButton.onClick.AddListener(() => {
                string str = inputField.text;
                if (string.IsNullOrWhiteSpace(str) || str.Contains(' ')) {
                    return;
                }

                text.text = str;
                propertyExplorer.gameObject.SetActive(false);
                int index = 0;
                for (int i = 0; i < parentTransform.childCount; ++i) {
                    if (parentTransform.GetChild(i) == property.transform) {
                        index = i;
                        break;
                    }
                }
                _node.Properties[index] = str;
            });

            property.onClick.AddListener(() => {
                if (propertyExplorer.gameObject.activeSelf) {
                    return;
                }

                propertyExplorer.gameObject.SetActive(true);
                inputField.text = text.text;
                inputField.ActivateInputField();
            });
        }
        public void RemovePropertyTemplate()
        {
            var parentTransform = PropertiesRoot;
            int childCount = parentTransform.transform.childCount;
            if (childCount > 0) {
                Destroy(parentTransform.GetChild(childCount - 1).gameObject);
                _node.Properties.RemoveAt(_node.Properties.Count - 1);
            }
        }
    }
}