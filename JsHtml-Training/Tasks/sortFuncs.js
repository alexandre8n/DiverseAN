        function stdSort(inArr) {
            let arr1 = [...inArr];
            arr1.sort((a, b) => a - b);
            return arr1;
        }

        function simpleSort(inArr) {
            let arr1 = [...inArr];
            let resArr = [];
            getMinIdx = (arr, i, len) => {
                let min1i = i;
                for (let j = i + 1; j < len; j++) {
                    if (arr[j] < arr[min1i]) min1i = j;
                }
                return min1i;
            }
            for (let i = 0; i < arr1.length; i++) {
                let minIdx = getMinIdx(arr1, i, arr1.length);
                resArr.push(arr1[minIdx]);
                if (i < minIdx) {
                    arr1[minIdx] = arr1[i];
                }
            }
            return resArr;
        }

        function quickSort(inArr) {
            let nums = [...inArr];
            // from here: https://www.programiz.com/dsa/quick-sort
            quickSortHelper(nums, 0, nums.length - 1);
            return nums;
        }

        function quickSortHelper(array, lowIndex, highIndex) {
            if (lowIndex >= highIndex) return;

            // find pivot element such that
            // elements smaller than pivot are on the left
            // elements greater than pivot are on the right
            // pi is index of pivot element
            let pi = quickSortPartition(array, lowIndex, highIndex);
            // recursive call on the left of pivot
            quickSortHelper(array, lowIndex, pi - 1);
            // recursive call on the right of pivot
            quickSortHelper(array, pi + 1, highIndex);
        }
        function quickSortPartition(array, lowIndex, highIndex) {
            // this function returns the index of pivot element
            // It takes pivot at the end (at highIndex) and makes in array all elements
            // lower than pivot, then pivot, then higher than pivot
            // choose the rightmost element as pivot
            let pivot = array[highIndex];
            // example: 5,6,8,2,0,3 - 3 is pivot
            // 1st swap: 2,6,8,5,0,3
            // 2nd swap: 2,0,8,5,6,3
            // 3rd swap: 2,0,3,5,6,8 -> return pivot idx=2

            // pointer for greater element
            let i = (lowIndex - 1);
            // traverse through all elements
            // compare each element with pivot
            for (let j = lowIndex; j < highIndex; j++) {
                if (array[j] <= pivot) {

                    // if element smaller than pivot is found
                    // swap it with the greatr element pointed by i
                    i++;

                    // swapping element at i with element at j
                    swap2(array, i, j);
                }
            }
            // swapt the pivot element with the greater element specified by i
            swap2(array, i + 1, highIndex);
            // return the position from where partition is done
            return (i + 1);
        }
        
        function sortBuble(arr1) {
            let arr = [...arr1]
            let icur = 1
            for (let i = 0; i < arr.length;) {
                if (arr[i] > arr[icur]) {
                    swap2(arr, i, icur)
                }
                icur++
                if (icur == arr.length) {
                    i++
                    icur = i + 1
                }
                if (icur > arr.length - 1) break;
            }
            return arr

        }
