namespace SmartX.Shared;

// one node in a deployment tree, e.g. Facility A contains Zone 1 which contains
// Sub-Zone B, sensors get registered against the lowest level node
public class DeploymentNode
{
    public string Name { get; set; }
    public List<DeploymentNode> Children { get; set; } = new();

    public DeploymentNode(string name)
    {
        Name = name;
    }
}

public static class DeploymentValidator
{
    // walks down the tree checking every node has a proper name, the base case
    // is a node with no children so this stops on its own and cant stack overflow
    public static bool ValidateNode(DeploymentNode node)
    {
        if (string.IsNullOrWhiteSpace(node.Name))
        {
            return false;
        }

        if (node.Children.Count == 0)
        {
            return true;
        }

        foreach (var child in node.Children)
        {
            if (!ValidateNode(child))
            {
                return false;
            }
        }

        return true;
    }

    // counts how many nodes are under this one including itself, also recursive
    public static int CountNodes(DeploymentNode node)
    {
        int total = 1;
        foreach (var child in node.Children)
        {
            total += CountNodes(child);
        }
        return total;
    }
}
