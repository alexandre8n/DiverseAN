// https://en.wikipedia.org/wiki/Binary_search_tree

let node = { key: 10, left: null, right: null, parent: null };
let binTree = { root: null };
addLeaf(binTree, 7);
addLeaf(binTree, 11);
addLeaf(binTree, 9);
addLeaf(binTree, 5);
addLeaf(binTree, 3);
addLeaf(binTree, 4);
addLeaf(binTree, 2);
addLeaf(binTree, 25);
addLeaf(binTree, 12);
addLeaf(binTree, 11.5);
addLeaf(binTree, 11.8);
addLeaf(binTree, 8.5);
addLeaf(binTree, 11.69);
addLeaf(binTree, 8.7);

let keyOfNode = 7;
let keyToTest =11.69;
let nodeToTest = findLeafRecursive(binTree.root, keyOfNode);
//let nodeSuc1 = findPredecessor2(nodeToTest, keyToTest);
//let nodePre1 = nodePredecessor(nodeToTest);
let nodeSuc = findSuccessor(nodeToTest, keyToTest);
let keys = getSortedArray(binTree.root);
let nd = findLeafRecursive(binTree.root, 11);
//deleteNode(binTree, nd);
let keys1 = getSortedArray(binTree.root);

let nodeToTestPre = findPredecessor(nodeToTest, keyToTest);
nodePre = keyPredecessorNode(nodeToTest, keyToTest);
let nodePre2 = findPredecessor2(nodeToTest, keyToTest);
console.log(nodeToTestPre);

//let nodeSuc = nodeSuccessor(nodeToTest);
//console.log(nodeSuc, nodePre);

function addLeaf(binTree, key) {
  let parentNode = null;
  let x = binTree.root;
  let leafToInsert = { key: key, left: null, right: null, parent: null };

  // find appropriate parent node
  while (x != null) {
    parentNode = x;
    if (key < x.key) x = x.left;
    else x = x.right;
  }
  leafToInsert.parent = parentNode;
  if (parentNode == null) binTree.root = leafToInsert;
  else if (leafToInsert.key < parentNode.key) parentNode.left = leafToInsert;
  else parentNode.right = leafToInsert;
}

function findLeafRecursive(node0, key) {
  if (node0 == null || key == node0.key) return node0;
  if (key < node0.key) return findLeafRecursive(node0.left, key);
  return findLeafRecursive(node0.right, key);
}
function findLeafIterative(node, key) {
  while (node != null && key != node.key) {
    if (key < node.key) node = node.left;
    else node = node.right;
  }
  return node;
}

// nearest previous by key value
// find node X with max key, so X.key < node.key
function findPredecessor(node, key) {
  if (node == null) return null;
  if (node.key < key) {
    let c1 = findMaxWithSmallerKey(node.right, key);
    // the following is true:
    // if node parent > key then all his right sub-tree > key
    //
    if (c1 == null && node.parent != null && node.parent.key >= key)
      return node;
    let bestCandidate = MaxOf2Nodes(node, c1);
    let par = getRightParentWithSmallerKey(node, key);
    while (par != null && par.key < key) {
      let c2 = findMaxWithSmallerKey(par, key);
      bestCandidate = MaxOf2Nodes(c2, bestCandidate);
      par = getRightParentWithSmallerKey(par, key);
    }
    return bestCandidate;
  }
  if (node.key >= key) {
    let c1 = findSmallerKey(node, key);
    c1 = findPredecessor(c1, key);
    return c1;
  }
}

function getLeftParent(node) {
  if (node == null || node.parent == null) return null;
  // Ok if this is the left parent, i.e. our node is right child
  if (node.parent.key < node.key) return node.parent;
  return getLeftParent(node.parent);
}
function getRightParentWithSmallerKey(node, key) {
  if (node.parent == null) return null;
  if (node.parent.key >= key) return null;
  // if left parent - he is smaller, all his right childs are
  // smaller then node, so we need to take his right parent
  if (node.parent.key < node.key)
    return getRightParentWithSmallerKey(node.parent, key);
  return node.parent;
}

function deleteNode(binSrcTree, nodeToDelete) {
  if (nodeToDelete.left == null) {
    shiftNodes(binSrcTree, nodeToDelete, nodeToDelete.right);
  } else if (nodeToDelete.right == null) {
    shiftNodes(binSrcTree, nodeToDelete, nodeToDelete.left);
  } else {
    E = nodeSuccessor(nodeToDelete);
    if (E.parent != nodeToDelete) {
      shiftNodes(binSrcTree, E, E.right);
      E.right = nodeToDelete.right;
      E.right.parent = E;
    }
    shiftNodes(binSrcTree, nodeToDelete, E);
    E.left = nodeToDelete.left;
    E.left.parent = E;
  }
}
function shiftNodes(binSrcTree, u, v) {
  if (u.parent == null) binSrcTree.root = v;
  else if (u == u.parent.left) u.parent.left = v;
  else u.parent.right = v;
  if (v != null) v.parent = u.parent;
}

// traverseInOrder ---------------------
function getSortedArray(node) {
  let keys = [];
  let todo = function (n) {
    keys.push(n.key);
  };
  visitInOrder(node, todo);
  return keys;
}
function visitInOrder(node, todoFunction) {
  if (node == null) return;
  visitInOrder(node.left, todoFunction);
  todoFunction(node);
  visitInOrder(node.right, todoFunction);
}

// traversePreOrder
// traversePostOrder

// service functions --------------------------------------
function MaxOf2Nodes(c1, c2) {
  if (!c1) return c2;
  if (!c2) return c1;
  return c1.key < c2.key ? c2 : c1;
}

// go left and if not succeeded
// go up to find a node with smaller key
function findSmallerKey(node, key) {
  if (node == null) return null;
  if (node.key < key) return node;
  let curNode = node;
  while (curNode != null) {
    let foundNode = findSmallerKeyLeft(curNode, key);
    if (foundNode != null && foundNode.key < key) return foundNode;
    curNode = getLeftParent(curNode, key);
  }
  return null;
}

// go right and find node with a key < input-key
// usage in case if node.key already < key
function findMaxWithSmallerKey(node, key) {
  if (node == null) return null;
  if (node.key == key) return nodePredecessor(node);
  let foundNode = null;
  while (node != null && node.key < key) {
    foundNode = node;
    node = node.right;
    if (node && node.key >= key) {
      let foundNode1 = findSmallerKeyLeft(node, key);
      if (foundNode1) return foundNode1;
    }
  }
  return foundNode;
}

// go down left from the node,
// find first node with key that less than input key
function findSmallerKeyLeft(node, key) {
  if (node == null) return null;
  let curNode = node.left;
  while (curNode != null) {
    if (curNode.key < key) return curNode;
    curNode = curNode.left;
  }
  return null;
}

function keyPredecessorNode(node, key) {
  if (node == null) return null;
  let curNode = node;
  let found = null;

  if (node.key < key) {
    while (curNode && curNode.key < key) {
      found = curNode;
      curNode = nodeSuccessor(curNode);
    }
    return found;
  }
  // node.key>=key
  while (curNode && curNode.key >= key) {
    curNode = nodePredecessor(curNode);
  }
  return curNode;
}

function nodePredecessor(x) {
  if (x.left != null) {
    return bstMaxToRight(x.left);
  }
  let y = x.parent;
  while (y != null && x == y.left) {
    x = y;
    y = y.parent;
  }
  return y;
}
function nodeSuccessor(x) {
  if (x.right) {
    return bstMinToLeft(x.right);
  }
  y = x.parent;
  while (y != null && x == y.right) {
    x = y;
    y = y.parent;
  }
  return y;
}

function bstMaxToRight(x) {
  while (x.right) x = x.right;
  return x;
}
function bstMinToLeft(x) {
  while (x.left) x = x.left;
  return x;
}
function findPredecessor2(node, key) {
  if (node == null) return;
  let predecessor = null;
  // find good root to start, in wirst case it will be root
  let current = findGoodParentToStart(node, key);
  // now current is a good root to start
  while (current !== null) {
    if (current.key === key) {
      // If the key is found, check if it has a left subtree
      // If yes, the predecessor is the rightmost node in the left subtree
      if (current.left !== null) {
        predecessor = current.left;
        while (predecessor.right !== null) {
          predecessor = predecessor.right;
        }
      }
      break;
    } else if (current.key < key) {
      // If current node value is less than key, move to the right subtree
      predecessor = current;
      current = current.right;
    } else {
      // If current node value is greater than key, move to the left subtree
      current = current.left;
    }
  }
  return predecessor;
}
function findSuccessor2(node, key) {
  if (node == null) return;
  let successor = null;
  // find good root to start, in wirst case it will be root
  let current = findGoodParentToStart(node, key);

  while (current !== null) {
    if (current.key === key) {
      // If the key is found, check if it has a right subtree
      // If yes, the successor is the leftmost node in the right subtree
      if (current.right !== null) {
        successor = current.right;
        while (successor.left !== null) {
          successor = successor.left;
        }
      }
      break;
    } else if (current.key < key) {
      // If current node value is less than key, move to the right subtree
      current = current.right;
    } else {
      // If current node value is greater than key, move to the left subtree
      successor = current;
      current = current.left;
    }
  }
  return successor;
}

// nearest next node by key value
// find node X with min key, so X.key > node.key
function findSuccessor(node, key) {
  let rootNode = findGoodParentToStart(node, key);

  // If the tree is empty, there is no successor

  if (rootNode == null) {
    return null;
  }

  // Initialize a variable to store the current node and its parent
  let currentNode = rootNode;
  let parentNode = null;
  // Traverse the tree to find the node with the given key
  while (currentNode != null) {
    if (key < currentNode.key) {
      parentNode = currentNode;
      currentNode = currentNode.left;
    } else if (key > currentNode.key) {
      currentNode = currentNode.right;
    } else {
      // If key is found, handle finding the successor
      break;
    }
  }

  // If the key is not found, return null
  if (currentNode == null && parentNode != null && parentNode.key > key) {
    return parentNode;
  }

  // If the right subtree is not empty, the successor is the leftmost node in the right subtree
  if (currentNode.right != null) {
    return getLeftmostNode(currentNode.right); // Helper function to find leftmost node
  }

  // Otherwise, the successor is the closest ancestor whose left child is also an ancestor of the current node
  while (parentNode != null && currentNode == parentNode.right) {
    currentNode = parentNode;
    parentNode = parentNode.parent;
  }

  return parentNode;
}

// Helper function to find the leftmost node in a subtree
function getLeftmostNode(node) {
  while (node.left != null) {
    node = node.left;
  }
  return node;
}

function findGoodParentToStart(node, key) {
  let minKey = node.key,
    maxKey = node.key;
  let current = node;
  while (current.parent != null) {
    [minKey, maxKey] = nodeMinMaxInfo(current, minKey, maxKey);
    if (key > minKey && key < maxKey) {
      break;
    }
    current = current.parent;
  }
  return current;
}
function nodeMinMaxInfo(node, min, max) {
  let nodeLeft = node.left ? node.left.key : node.key;
  let nodeRight = node.right ? node.right.key : node.key;
  min = Math.min(min, node.key, nodeLeft);
  max = Math.max(max, node.key, nodeRight);
  return [min, max];
}
