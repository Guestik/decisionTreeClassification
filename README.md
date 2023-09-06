# decisionTreeClassification
Decision Tree Classification in C#

Process details:

1. Initialization
  a. Set threshold (I set it to 0.3)
  b. Set minimal number of data for creating node (I set it to 10)
  c. Create instances of Feature object for each feature
  d. Import training dataset
    i. Create instance of DataElement object for each data
2. Building the tree
  a. Object “Node” is instantiated for each node of the tree recursively
    i. Impurity of all yet unused features are counted in the constructor and the smallest one is chosen to be feature of the node
    ii. If probability is smaller than threshold and the reduced dataset is greater than minimal, a new Left and Right node is created. Otherwise, it becomes a leaf. In case of the “restecg” feature, there are three possible outcomes. So for nodes with this particular feature, three children are created (for values 0, 1 and 2).
    iii. Instance of children nodes are held by each parent so the GC will not be triggered and it can be later used for testing purposes
3. Testing
  a. Import testing dataset
  b. Instance of “DataElement” is created for each data
  c. Each element is passed to the root of the tree and then classified recursively. When the process ends up in leaf, the corresponding value is returned
