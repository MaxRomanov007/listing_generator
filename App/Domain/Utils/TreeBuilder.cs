using System;
using System.Collections.Generic;
using System.Linq;

namespace App.Domain.Utils;

public class TreeBuilder
{
    private static readonly char[] Separator = ['/', '\\'];

    public static string BuildTree(IEnumerable<string> paths, string rootName = "")
    {
        var root = new TreeNode(rootName);
        
        foreach (var path in paths)
        {
            var segments = path.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            var currentNode = root;
            
            foreach (var segment in segments)
            {
                var child = currentNode.Children.FirstOrDefault(c => c.Name == segment);
                if (child == null)
                {
                    child = new TreeNode(segment);
                    currentNode.Children.Add(child);
                }
                currentNode = child;
            }
        }
        
        SortTree(root);
        return RenderTree(root);
    }
    
    private static void SortTree(TreeNode node)
    {
        node.Children.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        foreach (var child in node.Children)
        {
            SortTree(child);
        }
    }
    
    private static string RenderTree(TreeNode root)
    {
        var result = new List<string>();
        if (!string.IsNullOrEmpty(root.Name))
        {
            result.Add(root.Name);
        }
        
        RenderNode(result, root, "");
        return string.Join(Environment.NewLine, result);
    }
    
    private static void RenderNode(List<string> result, TreeNode node, string indent)
    {
        var lastChildIndex = node.Children.Count - 1;
        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var isLast = i == lastChildIndex;
            
            var prefix = isLast ? "└── " : "├── ";
            result.Add(indent + prefix + child.Name);
            
            var newIndent = indent + (isLast ? "    " : "│   ");
            RenderNode(result, child, newIndent);
        }
    }
    
    private class TreeNode(string name)
    {
        public string Name { get; } = name;
        public List<TreeNode> Children { get; } = [];
    }
}