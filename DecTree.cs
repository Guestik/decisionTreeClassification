using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DecTree
{
    class Feature
    {
        public string Name;
        public int Position;

        public Feature(string name, int position) //Constructor for creating new feature
        {
            Name = name;
            Position = position;
        }
    }

    class Node
    {
        private Node Right; //Child node with edge 1
        private Node Middle; //Child node with edge 2
        private Node Left; //Child node with edge 0
        private List<DataElement> leftReducedDataSet; //Training dataset that goes to 0 edge
        private List<DataElement> middleReducedDataSet; //Training dataset that goes to 2 edge
        private List<DataElement> rightReducedDataSet; //Training dataset that goes to 1 edge

        private Feature MyFeature; //This node's feature

        private List<Feature> NotUsedFeatures; //Not yet used features in this branch

        public string Testing(DataElement testingDataElement)
        {
            string result = null;
            if (testingDataElement.GetPropertyByNum(MyFeature.Position) == "0")
            {
                if (Left == null) //There is leaf in this edge
                {
                    //Return result according to the leaf's target probability
                    int nula = 0;
                    int jedna = 0;
                    for (int i = 0; i < leftReducedDataSet.Count; i++)
                    {
                        if (leftReducedDataSet[i].Target == "0")
                            nula++;
                        if (leftReducedDataSet[i].Target == "1")
                            jedna++;
                    }
                    if (nula > jedna)
                        return "0";
                    else
                        return "1";
                }
                else
                    result = Left.Testing(testingDataElement); //Continue to other node
            }
            else if (testingDataElement.GetPropertyByNum(MyFeature.Position) == "1")
            {
                if (Right == null) //There is leaf in this edge
                {
                    //Return result according to the leaf's target probability
                    int nula = 0;
                    int jedna = 0;
                    for (int i = 0; i < rightReducedDataSet.Count; i++)
                    {
                        if (rightReducedDataSet[i].Target == "0")
                            nula++;
                        if (rightReducedDataSet[i].Target == "1")
                            jedna++;
                    }
                    if (nula > jedna)
                        return "0";
                    else
                        return "1";
                }
                else
                    result = Right.Testing(testingDataElement); //Continue to other node
            }
            else if (testingDataElement.GetPropertyByNum(MyFeature.Position) == "2")
            {
                if (Middle == null) //There is leaf in this edge
                {
                    //Return result according to the leaf's target probability
                    int nula = 0;
                    int jedna = 0;
                    for (int i = 0; i < middleReducedDataSet.Count; i++)
                    {
                        if (middleReducedDataSet[i].Target == "0")
                            nula++;
                        if (middleReducedDataSet[i].Target == "1")
                            jedna++;
                    }
                    if (nula > jedna)
                        return "0";
                    else
                        return "1";
                }
                else
                    result = Middle.Testing(testingDataElement); //Continue to other node
            }
            return result;
        }

        public Node(List<DataElement> inputDataSet, List<Feature> notUsedFeatures, double threshold, int minNum)
        {
            NotUsedFeatures = new List<Feature>(notUsedFeatures);

            //Count the impurity and find this node's element
            double[] allImpurities = new double[NotUsedFeatures.Count];

            Feature toberemoved = null;
            for (int i = 0; i < NotUsedFeatures.Count; i++)
            {
                double temp = CalculateTotalImpurity(inputDataSet, NotUsedFeatures[i].Position);
                if (temp == 2) //This is like error code - the impurity can not be counted because one of the reduced datasets are empty
                {
                    allImpurities[i] = temp;
                    toberemoved = NotUsedFeatures[i];
                }
                else
                    allImpurities[i] = temp;
            }

            //Get the smallest value, create new reduced dataSet, remove used feature from notUsedFeatures and create new node
            int indexOfMin = Array.IndexOf(allImpurities, allImpurities.Min());
            leftReducedDataSet = GetLeftReducedDataset(inputDataSet, NotUsedFeatures[indexOfMin].Position);
            rightReducedDataSet = GetRightReducedDataset(inputDataSet, NotUsedFeatures[indexOfMin].Position);
            middleReducedDataSet = GetMiddleReducedDataset(inputDataSet, notUsedFeatures[indexOfMin].Position);
            
            //Assign feature to this node
            MyFeature = NotUsedFeatures[indexOfMin];

            //Remove this feature from list of not yet used features
            NotUsedFeatures.RemoveAt(indexOfMin);
            if (toberemoved != null)
                NotUsedFeatures.Remove(toberemoved);

            //Count probability of edge 0
            double leftProbability = (double)leftReducedDataSet.Count / (double)inputDataSet.Count;

            //If the probability is smaller than threshold and the reduced dataset is greater than minimal, create new node at this edge
            if (leftProbability > threshold && NotUsedFeatures.Count != 0 && leftReducedDataSet.Count > minNum)
                Left = new Node(leftReducedDataSet, NotUsedFeatures, threshold, minNum);

            //Count probability of edge 1
            double rightProbability = (double)rightReducedDataSet.Count / (double)inputDataSet.Count;

            //If the probability is smaller than threshold and the reduced dataset is greater than minimal, create new node at this edge
            if (rightProbability > threshold && NotUsedFeatures.Count != 0 && rightReducedDataSet.Count > minNum)
                Right = new Node(rightReducedDataSet, NotUsedFeatures, threshold, minNum);

            //If this is Restecg (only Restecg can have 3 children)
            if (MyFeature.Position == 6)
            {
                //Count probability of edge 2
                double middleProbability = (double)middleReducedDataSet.Count / (double)inputDataSet.Count;

                //If the probability is smaller than threshold and the reduced dataset is greater than minimal, create new node at this edge
                if (middleProbability > threshold && NotUsedFeatures.Count != 0 && middleReducedDataSet.Count > minNum)
                    Middle = new Node(middleReducedDataSet, NotUsedFeatures, threshold, minNum);
            }
        }

        public double CalculateTotalImpurity(List<DataElement> inputData, int property/*, out List<DataElement> nula, out List<DataElement> jedna*/)
        {
            //This method counts total impurity of given parameter
            if (property == 6) //For Restecg
            {
                //Restecg has different kid of data, because it can be 0 or 1 or 2
                List<DataElement> nula = GetLeftReducedDataset(inputData, property);
                List<DataElement> jedna = GetRightReducedDataset(inputData, property);
                List<DataElement> dva = GetMiddleReducedDataset(inputData, property);

                if (dva.Count == 0)
                    return 2;

                int parametrNulaNo = 0;
                int parametrNulaYes = 0;
                int parametrNulaMiddle = 0;
                for (int i = 0; i < nula.Count; i++)
                {
                    if (nula[i].Target == "0")
                        parametrNulaNo++;
                    if (nula[i].Target == "1")
                        parametrNulaYes++;
                    if (nula[i].Target == "2")
                        parametrNulaMiddle++;
                }

                double sexImpurityLeft = 1 - (double)Math.Pow((double)parametrNulaNo / (double)nula.Count, 2) - (double)Math.Pow((double)parametrNulaYes / (double)nula.Count, 2) - (double)Math.Pow((double)parametrNulaMiddle / (double)nula.Count, 2);

                int parametrJednaNo = 0;
                int parametrJednaYes = 0;
                int parametrJednaMiddle = 0;
                for (int i = 0; i < jedna.Count; i++)
                {
                    if (jedna[i].Target == "0")
                        parametrJednaNo++;
                    if (jedna[i].Target == "1")
                        parametrJednaYes++;
                    if (jedna[i].Target == "2")
                        parametrJednaMiddle++;
                }

                double sexImpurityRight = 1 - (double)Math.Pow((double)parametrJednaNo / (double)jedna.Count, 2) - (double)Math.Pow((double)parametrJednaYes / (double)jedna.Count, 2) - (double)Math.Pow((double)parametrJednaMiddle / (double)jedna.Count, 2);
                
                int parametrDvaNo = 0;
                int parametrDvaYes = 0;
                int parametrDvaMiddle = 0;
                for (int i = 0; i < dva.Count; i++)
                {
                    if (jedna[i].Target == "0")
                        parametrDvaNo++;
                    if (jedna[i].Target == "1")
                        parametrDvaYes++;
                    if (jedna[i].Target == "2")
                        parametrDvaMiddle++;
                }
                double sexImpurityMiddle = 1 - (double)Math.Pow((double)parametrDvaNo / (double)dva.Count, 2) - (double)Math.Pow((double)parametrDvaYes / (double)dva.Count, 2) - (double)Math.Pow((double)parametrDvaMiddle / (double)dva.Count, 2);

                double totalSexImpurity = ((double)nula.Count / inputData.Count) * sexImpurityLeft + ((double)jedna.Count / inputData.Count) * sexImpurityRight + ((double)dva.Count / inputData.Count) * sexImpurityMiddle;
                return totalSexImpurity;
            }
            else
            {
                List<DataElement> nula = GetLeftReducedDataset(inputData, property);
                List<DataElement> jedna = GetRightReducedDataset(inputData, property);

                int parametrNulaNo = 0;
                int parametrNulaYes = 0;
                for (int i = 0; i < nula.Count; i++)
                {
                    if (nula[i].Target == "0")
                        parametrNulaNo++;
                    if (nula[i].Target == "1")
                        parametrNulaYes++;
                }

                double sexImpurityLeft = 1 - (double)Math.Pow((double)parametrNulaNo / (double)nula.Count, 2) - (double)Math.Pow((double)parametrNulaYes / (double)nula.Count, 2);

                int parametrJednaNo = 0;
                int parametrJednaYes = 0;
                for (int i = 0; i < jedna.Count; i++)
                {
                    if (jedna[i].Target == "0")
                        parametrJednaNo++;
                    if (jedna[i].Target == "1")
                        parametrJednaYes++;
                }
                double sexImpurityRight = 1 - (double)Math.Pow((double)parametrJednaNo / (double)jedna.Count, 2) - (double)Math.Pow((double)parametrJednaYes / (double)jedna.Count, 2);

                double totalSexImpurity = ((double)nula.Count / inputData.Count) * sexImpurityLeft + ((double)jedna.Count / inputData.Count) * sexImpurityRight;
                return totalSexImpurity;
            }
        }

        private List<DataElement> GetLeftReducedDataset(List<DataElement> inputData, int property)
        {
            //Get reduced dataset tht goes to the edge 0
            List<DataElement> left = new List<DataElement>();

            for (int i = 0; i < inputData.Count; i++)
            {
                if (inputData[i].GetPropertyByNum(property) == "0")
                    left.Add(inputData[i]);
            }
            return left;
        }

        private List<DataElement> GetRightReducedDataset(List<DataElement> inputData, int property)
        {
            //Get reduced dataset tht goes to the edge 1
            List<DataElement> right = new List<DataElement>();

            for (int i = 0; i < inputData.Count; i++)
            {
                if (inputData[i].GetPropertyByNum(property) == "1")
                    right.Add(inputData[i]);
            }
            return right;
        }

        private List<DataElement> GetMiddleReducedDataset(List<DataElement> inputData, int property)
        {
            //Get reduced dataset tht goes to the edge 2 in case of restecg
            List<DataElement> middle = new List<DataElement>();

            for (int i = 0; i < inputData.Count; i++)
                if (inputData[i].GetPropertyByNum(property) == "2")
                    middle.Add(inputData[i]);
            return middle;
        }
    }

    class DataElement
    {
        public string Sex { get; private set; }
        public string Fbs { get; private set; }
        public string Restecg { get; private set; }
        public string Exang { get; private set; }
        public string Target { get; private set; }

        public DataElement(string sex, string fbs, string restecg, string exang, string target) //Constructor for training data element (with target)
        {
            this.Sex = sex;
            this.Fbs = fbs;
            this.Restecg = restecg;
            this.Exang = exang;
            this.Target = target;
        }

        public DataElement(string sex, string fbs, string restecg, string exang) //Constructor for testing data element (without target)
        {
            this.Sex = sex;
            this.Fbs = fbs;
            this.Restecg = restecg;
            this.Exang = exang;
        }

        public void SetTarget(string target) //To assign target for testing data
        {
            this.Target = target;
        }

        public string GetPropertyByNum(int numOfTheProperty)
        {
            if (numOfTheProperty == 1)
                return Sex;
            if (numOfTheProperty == 5)
                return Fbs;
            if (numOfTheProperty == 6)
                return Restecg;
            if (numOfTheProperty == 8)
                return Exang;
            else
                return null;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            //Set threshold
            double threshold = 0.3f;

            //Set minimal number of data for creating node
            int minNum = 10;

            //Features not used yet (in the particular branch)
            List<Feature> notUsedFeatures = new List<Feature>();

            //Create features
            notUsedFeatures.Add(new Feature("Sex", 1));
            notUsedFeatures.Add(new Feature("Fbs", 5));
            notUsedFeatures.Add(new Feature("Restecg", 6));
            notUsedFeatures.Add(new Feature("Exang", 8));

            //Import the training dataset
            List<DataElement> inputData = new List<DataElement>();
            using (StreamReader reader = new StreamReader("training_data_tree.txt"))
            {
                string line;
                //Read line by line  
                while ((line = reader.ReadLine()) != null)
                {
                    string[] split = line.Split(';');
                    //Create instance of DataElement object for each data
                    inputData.Add(new DataElement(split[notUsedFeatures[0].Position].Trim(), split[notUsedFeatures[1].Position].Trim(), split[notUsedFeatures[2].Position].Trim(), split[notUsedFeatures[3].Position].Trim(), split[split.Length - 1].Trim()));
                }
            }

            //Start building the tree here
            Node rootNode = new Node(inputData, notUsedFeatures, threshold, minNum);

            //Import the testing dataset
            List<DataElement> testingData = new List<DataElement>();
            using (StreamReader reader = new StreamReader("testing_data_tree.txt"))
            {
                string line;
                //Read line by line
                while ((line = reader.ReadLine()) != null)
                {
                    string[] split = line.Split(';');
                    testingData.Add(new DataElement(split[notUsedFeatures[0].Position].Trim(), split[notUsedFeatures[1].Position].Trim(), split[notUsedFeatures[2].Position].Trim(), split[notUsedFeatures[3].Position].Trim()));
                }
            }

            //Test data
            for (int i = 0; i < testingData.Count(); i++)
                testingData[i].SetTarget(rootNode.Testing(testingData[i]));

            //Export the data
            string[] arrLine = File.ReadAllLines("original_data.txt");
            for (int i = 0; i < testingData.Count(); i++)
            {
                arrLine[i] += String.Format("\t" + testingData[i].Target);
                File.WriteAllLines("testing_results_tree.txt", arrLine);
            }
        }
    }
}
