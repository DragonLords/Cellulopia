using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// classe qui sert a faicliter lutilisation des tags
/// en utilisant [TagSelector] et comme type string
/// on peut alors avoir access a un dropdown dans linspecteur pour faciliter cette methode de faire
/// </summary>
[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorPropertyDrawer : PropertyDrawer
{
    /// <summary>
    /// ici on appelle la focntion de basse de leditor OnGUI mais en override
    /// </summary>
    /// <param name="position"></param>
    /// <param name="property"></param>
    /// <param name="label"></param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //si la propriete est de type string alors on peut commencer (cest pour eviter les erreur avec int ou layermask ou GameObject)
        if (property.propertyType == SerializedPropertyType.String)
        {
            //vu que cest un string alors on commencea definir comment la propriete doit etre
            EditorGUI.BeginProperty(position, label, property);

            //on garde facilement accessible la propriete
            var attrib = this.attribute as TagSelectorAttribute;

            if (attrib.UseDefaultTagFieldDrawer)
            {
                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            }
            else
            {
                //genere une liste qui contient tout les tags de base et custom
                List<string> tagList = new List<string>();
                //avant tout on met le tag "NoTag" qui donnera une erreur si lon assigne pas de tag
                tagList.Add("<NoTag>");
                //alors on ajoute tout les atg present dans unity
                tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);
                //on va chercher sous string la proprieter quon modifie
                string propertyString = property.stringValue;
                int index = -1;
                //detecte si le tag est vide
                if (propertyString == "")
                {
                    //le premier index est le fameux tag <notag>
                    index = 0; 
                }
                else
                {
                    //on regarde si il y a une entre qui correcponde a ce quon cherche et va chercher lindex
                    //on skip lindex 0 qui est le <notag>
                    for (int i = 1; i < tagList.Count; i++)
                    {
                        if (tagList[i] == propertyString)
                        {
                            index = i;
                            break;
                        }
                    }
                }

                //Dessine le dropdown en popup pour le selectionneur de tag
                index = EditorGUI.Popup(position, label.text, index, tagList.ToArray());

                //On modifie le string du script avec lattribut [TagSelector] avec la velur necessaire
                if (index == 0)
                {
                    property.stringValue = "";
                }
                else if (index >= 1)
                {
                    property.stringValue = tagList[index];
                }
                else
                {
                    property.stringValue = "";
                }
            }

            EditorGUI.EndProperty();
        }
        //sinon on affiche la proprieter tel que sans dropdown
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif