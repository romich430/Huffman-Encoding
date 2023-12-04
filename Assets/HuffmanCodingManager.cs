using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;
using TMPro;
using SimpleFileBrowser;

public class HuffmanCodingManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI resultBit;
    public TextMeshProUGUI textBit;
    public TextMeshProUGUI result;

    private void Start()
    {
        FileBrowser.CheckPermission();
    }

    public void OnEncodeButtonPressed(string text)
    {
        //Setup
        string input = "";
        if (!string.IsNullOrEmpty(text))
        {
            input = text;
            inputField.text = text;
        }
        else
        {
            input = inputField.text;
        }
        HuffmanTree huffmanTree = new HuffmanTree();
        huffmanTree.Build(input);
        
        //Encoding
        BitArray encoded = huffmanTree.Encode(input);
        string res = "";
        foreach (bool bit in encoded)
        {
            res += (bit ? 1 : 0).ToString();
        }
        resultBit.text = res;
        
        //Decoding
        string inputBit = textBit.text;
        BitArray bitArrayInput = new BitArray(inputBit.Select(c => c == '1').ToArray());
        string decoded = huffmanTree.Decode(bitArrayInput);
        result.text = decoded;
    }

    public void OnImportButtonPressed()
    {
        StartCoroutine(ShowImportDialog());
    }

    public void OnExportButtonPressed()
    {
        StartCoroutine(ShowExportDialog());
    }

    public void OnAboutButtonPressed()
    {
        Application.OpenURL("https://en.wikipedia.org/wiki/Huffman_coding");
    }

    IEnumerator ShowExportDialog()
    {
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Text Files", ".txt"));
        FileBrowser.SetDefaultFilter(".txt");
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Folders, false, null, "",
            "Export", "Save");
        if (FileBrowser.Success)
        {
            string path = FileBrowser.Result[0];
            Debug.Log(path);

            string result = "Input text: " + inputField.text + "\n" + "Encoded: " + resultBit.text;
            Debug.Log(result);
            FileBrowserHelpers.WriteTextToFile(FileBrowserHelpers.CreateFileInDirectory(FileBrowser.Result[0], "Result.txt"), result);
        }
    }

    IEnumerator ShowImportDialog()
    {
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Text Files", ".txt"));
        FileBrowser.SetDefaultFilter(".txt");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Load", "Select");
        if (FileBrowser.Success)
        {
            byte[] result = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            //string res = BitConverter.ToString(result);
            string res = Encoding.UTF8.GetString(result);
            OnEncodeButtonPressed(res);
            Debug.Log(res);
        }
    }
}

public class Node
    {
        public char Symbol { get; set; }
        public int Frequency { get; set; }
        public Node Right { get; set; }
        public Node Left { get; set; }

        public List<bool> Traverse(char symbol, List<bool> data)
        {
            // Leaf
            if (Right == null && Left == null)
            {
                if (symbol.Equals(this.Symbol))
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                List<bool> left = null;
                List<bool> right = null;

                if (Left != null)
                {
                    List<bool> leftPath = new List<bool>();
                    leftPath.AddRange(data);
                    leftPath.Add(false);

                    left = Left.Traverse(symbol, leftPath);
                }

                if (Right != null)
                {
                    List<bool> rightPath = new List<bool>();
                    rightPath.AddRange(data);
                    rightPath.Add(true);
                    right = Right.Traverse(symbol, rightPath);
                }

                if (left != null)
                {
                    return left;
                }
                else
                {
                    return right;
                }
            }
        }
    }

public class HuffmanTree
    {
        private List<Node> nodes = new List<Node>();
        public Node Root { get; set; }
        public Dictionary<char, int> Frequencies = new Dictionary<char, int>();

        public void Build(string source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (!Frequencies.ContainsKey(source[i]))
                {
                    Frequencies.Add(source[i], 0);
                }

                Frequencies[source[i]]++;
            }

            foreach (KeyValuePair<char, int> symbol in Frequencies)
            {
                nodes.Add(new Node() { Symbol = symbol.Key, Frequency = symbol.Value });
            }

            while (nodes.Count > 1)
            {
                List<Node> orderedNodes = nodes.OrderBy(node => node.Frequency).ToList<Node>();

                if (orderedNodes.Count >= 2)
                {
                    // Take first two items
                    List<Node> taken = orderedNodes.Take(2).ToList<Node>();

                    // Create a parent node by combining the frequencies
                    Node parent = new Node()
                    {
                        Symbol = '*',
                        Frequency = taken[0].Frequency + taken[1].Frequency,
                        Left = taken[0],
                        Right = taken[1]
                    };

                    nodes.Remove(taken[0]);
                    nodes.Remove(taken[1]);
                    nodes.Add(parent);
                }

                this.Root = nodes.FirstOrDefault();

            }

        }

        public BitArray Encode(string source)
        {
            List<bool> encodedSource = new List<bool>();

            for (int i = 0; i < source.Length; i++)
            {
                List<bool> encodedSymbol = this.Root.Traverse(source[i], new List<bool>());
                encodedSource.AddRange(encodedSymbol);
            }

            BitArray bits = new BitArray(encodedSource.ToArray());

            return bits;
        }

        public string Decode(BitArray bits)
        {
            Node current = this.Root;
            string decoded = "";

            foreach (bool bit in bits)
            {
                if (bit)
                {
                    if (current.Right != null)
                    {
                        current = current.Right;
                    }
                }
                else
                {
                    if (current.Left != null)
                    {
                        current = current.Left;
                    }
                }

                if (IsLeaf(current))
                {
                    decoded += current.Symbol;
                    current = this.Root;
                }
            }

            return decoded;
        }

        public bool IsLeaf(Node node)
        {
            return (node.Left == null && node.Right == null);
        }

    }

