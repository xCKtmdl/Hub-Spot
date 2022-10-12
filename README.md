# HubSpot Technical Interview exercise

This is the hubspot technical code test. It was transliterated from a python version of the code I found elsewhere on github.

After I passed the test, here is the question they asked me in my video interview.

Merge two sorted lists.

Here is the code I gave:

```csharp
        public static int[] mergeSortedArrays1(int[] arr1, int[] arr2)
        {
            List<int> list = new List<int>();
            foreach (var numArr1 in arr1.ToList())
            {
                list.Add(numArr1);
            }
            foreach (var numArr2 in arr2.ToList())
            {
                list.Add(numArr2);
            }
            list.Sort();
            return list.ToArray();
        }
```

They wanted me to make use of the fact that the arrays are already sorted, so
 they were looking for something more like this:
 
 ```csharp
        public static int[] mergeSortedArrays2(int[] arr1, int[] arr2)
        {
            int n1 = arr1.Length;
            int n2 = arr2.Length;
            int[] retArray = new int[n1+n2];
            int i=0, j=0, k=0;

            while (i < n1 && j < n2)
            {
                if (arr1[i] < arr2[j])
                    retArray[k++] = arr1[i++];
                else
                    retArray[k++] = arr2[j++];
            }
            while (i < n1)
                retArray[k++] = arr1[i++];
            while (j < n2)
                retArray[k++] = arr2[j++];
            return retArray;
        }
 ```