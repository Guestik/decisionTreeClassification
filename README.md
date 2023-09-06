# decisionTreeClassification
## Decision Tree Classification in C#

Process details:

1. Initialization
  - Set threshold (I set it to 0.3)
  - Set minimal number of data for creating node (I set it to 10)
  - Create instances of Feature object for each feature
  - Import training dataset
    - Create instance of DataElement object for each data
2. Building the tree
  - Object “Node” is instantiated for each node of the tree recursively
    - Impurity of all yet unused features are counted in the constructor and the smallest one is chosen to be feature of the node
    - If probability is smaller than threshold and the reduced dataset is greater than minimal, a new Left and Right node is created. Otherwise, it becomes a leaf. In case of the “restecg” feature, there are three possible outcomes. So for nodes with this particular feature, three children are created (for values 0, 1 and 2).
    - Instance of children nodes are held by each parent so the GC will not be triggered and it can be later used for testing purposes
3. Testing
  - Import testing dataset
  - Instance of “DataElement” is created for each data
  - Each element is passed to the root of the tree and then classified recursively. When the process ends up in leaf, the corresponding value is returned
