var logger = (typeof logger !== 'undefined' && logger) || console;
; if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }; //var dnn = dnn || {};

(function ($, window, document, undefined) {
    "use strict";

    var NTree = this.NTree = function (data) {
		logger.error(`>>> Entering NTree(data=${data})`, { "event.action": "NTree", "logrite.data": data });
        this.data = (typeof data === "undefined") ? null : data;
        this.children = [];
		logger.debug(`NTree(data=${data}): this.children → ${this.children}`, { "event.action": "NTree", "logrite.data": data });
    };

    NTree.prototype = {

        constructor: NTree,

        addChildNode: function (node) {
            this.children.push(node);
			logger.warn(`<<< Exiting addChildNode(node=${node})`, { "event.action": "addChildNode", "logrite.node": node });
            return this;
        },

        addChildNodes: function (nodes) {
			logger.error(`>>> Entering addChildNodes(nodes=${nodes})`, { "event.action": "addChildNodes", "logrite.nodes": nodes });
            this.children = this.children.concat(nodes);
			logger.warn(`<<< Exiting addChildNodes(nodes=${nodes})`, { "event.action": "addChildNodes", "logrite.nodes": nodes });
            return this;
        },

        hasNode: function (node) {
			logger.error(`>>> Entering hasNode(node=${node})`, { "event.action": "hasNode", "logrite.root": root, "logrite.node": node });
			logger.warn(`<<< Exiting hasNode(node=${node})`, { "event.action": "hasNode", "logrite.root": root, "logrite.node": node });
            return NTree.hasNode(this, node);
        },

        removeChildren: function () {
			logger.error('>>> Entering removeChildren()', { "event.action": "removeChildren" });
            for (var i = 0, count = this.children.length; i < count; i++) {
                var child = this.children[i];
                if (child) {
                    child.removeChildren();
                }
            }
            this.children = [];
			logger.warn('<<< Exiting removeChildren()', { "event.action": "removeChildren" });
			logger.debug(`removeChildren(): this.children → ${this.children}`, { "event.action": "removeChildren" });
            return this;
        },

        getNumberOfChildren: function (recursive) {
			logger.error(`>>> Entering getNumberOfChildren(recursive=${recursive})`, { "event.action": "getNumberOfChildren", "logrite.recursive": recursive });
            var n = this.children.length;
            if (recursive) {
                for (var i = 0, count = this.children.length; i < count; i++) {
                    var child = this.children[i];
                    if (child) {
                        n += child.getNumberOfChildren(recursive);
						logger.warn(`<<< Exiting getNumberOfChildren(recursive=${recursive})`, { "event.action": "getNumberOfChildren", "logrite.recursive": recursive });
                    }
                }
            }
            return n;
        },

        hasChildren: function () {
			logger.error('>>> Entering hasChildren()', { "event.action": "hasChildren" });
			logger.warn('<<< Exiting hasChildren()', { "event.action": "hasChildren" });
            return (this.children && this.children.length > 0);
        },

        inOrderTraverse: function (callback) {
			logger.error(`>>> Entering inOrderTraverse(callback=${callback})`, { "event.action": "inOrderTraverse", "logrite.root": root, "logrite.callback": callback });
			logger.warn(`<<< Exiting inOrderTraverse(callback=${callback})`, { "event.action": "inOrderTraverse", "logrite.root": root, "logrite.callback": callback });
            return NTree.inOrderTraverse(this, callback);
        }

    };

    NTree.equals = function (one, another, nodeComparer) {
        if (one === another) {
            return true;
        }
        return typeof nodeComparer === "function" && one && another && nodeComparer.call(this, one.data, another.data);
    };

    NTree.sort = function (root, comparer) {
		logger.error(`>>> Entering sort(root=${root},comparer=${comparer})`, { "event.action": "sort", "logrite.root": root, "logrite.comparer": comparer });
        if (!(root && comparer)) {
            return;
        }
        root.children.sort(comparer);
        for (var i = 0, count = root.children.length; i < count; i++) {
            NTree.sort(root.children[i], comparer);
        }
		logger.warn(`<<< Exiting sort(root=${root},comparer=${comparer})`, { "event.action": "sort", "logrite.root": root, "logrite.comparer": comparer });
    };

    NTree.hasNode = function (root, node) {
		logger.error(`>>> Entering hasNode(root=${root},node=${node})`, { "event.action": "hasNode", "logrite.root": root, "logrite.node": node });
        if (!(root && node)) {
            return false;
        }
        if (root === node) {
            return true;
        }
        for (var i = 0, count = root.children.length; i < count; i++) {
            if (NTree.hasNode(root.children[i], node)) {
                return true;
            }
        }
		logger.warn(`<<< Exiting hasNode(node=${node})`, { "event.action": "hasNode", "logrite.root": root, "logrite.node": node });
        return false;
    };

    NTree.isEmpty = function (node) {
        if (!node) {
            return true;
        }
        return (!node.data && (!node.children || node.children.length === 0));
    };

    NTree.inOrderTraverse = function (root, callback) {
		logger.error(`>>> Entering inOrderTraverse(root=${root},callback=${callback})`, { "event.action": "inOrderTraverse", "logrite.root": root, "logrite.callback": callback });
        if (!root) {
            return false;
        }
        var node = root;
        var stack = [];
        var indexStack = [];
        var index = 0;
        while (true) {
            while (node) {
                if (typeof callback === "function" && callback.call(this, node, stack)) {
                    stack = [];
                    indexStack = [];
                    break;
                }
                if (node.children.length > 0) {
                    stack.push(node);
                    indexStack.push(0);
                    node = node.children[0];
                }
                else {
                    node = null;
                }
            }
            if (stack.length === 0) {
                break;
            }
            node = stack.pop();
            index = indexStack.pop();
            if (node.children.length - 1 > index) {
                index += 1;
                stack.push(node);
                indexStack.push(index);
                node = node.children[index];
            }
            else {
                node = null;
            }
        }
		logger.warn(`<<< Exiting inOrderTraverse(callback=${callback})`, { "event.action": "inOrderTraverse", "logrite.root": root, "logrite.callback": callback });
        return true;
    };

    NTree.getPath = function (root, predicate) {
		logger.error(`>>> Entering getPath(root=${root},predicate=${predicate})`, { "event.action": "getPath", "logrite.root": root, "logrite.predicate": predicate });
        var path = [];
		logger.error(`getPath(root=${root},predicate=${predicate}): path → ${path}`, { "event.action": "getPath", "logrite.root": root, "logrite.predicate": predicate });
        var callback = function (traversedNode, stack) {
			logger.error(`>>> Entering callback(traversedNode=${traversedNode},stack=${stack})`, { "event.action": "callback", "logrite.traversedNode": traversedNode, "logrite.stack": stack });
            if (typeof predicate === "function" && predicate.call(this, traversedNode.data)) {
                path = stack.slice(0); // clones array
                path.push(traversedNode); // adds the current node
                return true;
            }
			logger.warn(`<<< Exiting callback(traversedNode=${traversedNode},stack=${stack})`, { "event.action": "callback", "logrite.traversedNode": traversedNode, "logrite.stack": stack });
            return false;
        };
        NTree.inOrderTraverse(root, callback);
        return path;
    };

    var Dictionary = this.Dictionary = function (elements) {
		logger.error(`>>> Entering Dictionary(elements=${elements})`, { "event.action": "Dictionary", "logrite.elements": elements });
        this._elements = elements || {};
        this.hasSpecialProto = false; // has "__proto__" key
        this.specialProto = undefined; // "__proto__" element
		logger.debug(`Dictionary(elements=${elements}): this._elements → ${this._elements}`, { "event.action": "Dictionary", "logrite.elements": elements });
    };

    Dictionary.prototype = {

        constructor: Dictionary,

        has: function (key) {
			logger.error(`>>> Entering has(key=${key})`, { "event.action": "has", "logrite.key": key });
            if (key === "__proto__") {
                return this.hasSpecialProto;
            }
			logger.warn(`<<< Exiting has(key=${key})`, { "event.action": "has", "logrite.key": key });
            return {}.hasOwnProperty.call(this._elements, key);
        },

        get: function (key) {
			logger.error(`>>> Entering get(key=${key})`, { "event.action": "get", "logrite.key": key });
            if (key === "__proto__") {
                return this.specialProto;
            }
			logger.warn(`<<< Exiting get(key=${key})`, { "event.action": "get", "logrite.key": key });
            return this.has(key) ? this._elements[key] : undefined;
        },

        set: function (key, value) {
			logger.error(`>>> Entering set(key=${key},value=${value})`, { "event.action": "set", "logrite.key": key, "logrite.value": value });
            if (Object.isNullOrUndefined(key)) {
                throw "InvalidArgumentException {" + key + "},{" + value + "}";
            }
            if (key === "__proto__") {
                this.hasSpecialProto = true;
                this.specialProto = value;
				logger.debug(`set(key=${key},value=${value}): this.hasSpecialProto → ${this.hasSpecialProto}`, { "event.action": "set", "logrite.key": key, "logrite.value": value });
            }
            else {
				logger.debug(`set(key=${key},value=${value}): this.specialProto → ${this.specialProto}`, { "event.action": "set", "logrite.key": key, "logrite.value": value });
                this._elements[key] = value;
				logger.warn(`<<< Exiting set(key=${key},value=${value})`, { "event.action": "set", "logrite.key": key, "logrite.value": value });
            }
        },

        remove: function(key) {
			logger.error(`>>> Entering remove(key=${key})`, { "event.action": "remove" });
            if (key === "__proto__") {
                this.hasSpecialProto = false;
                this.specialProto = undefined;
				logger.debug(`remove(key=${key}): this.hasSpecialProto → ${this.hasSpecialProto}`, { "event.action": "remove" });
            }
            else {
				logger.debug(`remove(key=${key}): this.specialProto → ${this.specialProto}`, { "event.action": "remove" });
                delete this._elements[key];
            }
			logger.error('>>> Entering remove()', { "event.action": "remove" });
			logger.warn('<<< Exiting remove()', { "event.action": "remove" });
        },

        clear: function() {
			logger.error('>>> Entering clear()', { "event.action": "clear" });
            this._elements = [];
			logger.debug(`clear(): this._elements → ${this._elements}`, { "event.action": "clear" });
        },

        keys: function() {
			logger.error('>>> Entering keys()', { "event.action": "keys" });
            var names = Object.getOwnPropertyNames(this._elements);
            if (this.hasSpecialProto) {
				logger.error(`keys(): names → ${names}`, { "event.action": "keys" });
                names.push("__proto__");
				logger.warn('<<< Exiting keys()', { "event.action": "keys" });
            }
            return names;
        },

        entries: function() {
			logger.error('>>> Entering entries()', { "event.action": "entries" });
			logger.warn('<<< Exiting entries()', { "event.action": "entries" });
            return this._elements;
        }

    };


    var MinHeap = this.MinHeap = function(array, comparator) {
	logger.error(`>>> Entering MinHeap(array=${array},comparator=${comparator})`, { "event.action": "MinHeap", "logrite.array": array, "logrite.comparator": comparator });

        this.heap = array || [];

		logger.debug(`MinHeap(array=${array},comparator=${comparator}): this.heap → ${this.heap}`, { "event.action": "MinHeap", "logrite.array": array, "logrite.comparator": comparator });
        /**
         * Default comparator used if an override is not provided.
         * @private
         */
        this.compare = comparator || function(one, another) {
            return one == another ? 0 : one < another ? -1 : 1;
        };

        /**
         * Retrieve the index of the left child of the node at index i.
         */
        this._left = function(index) {
			logger.error(`>>> Entering _left(index=${index})`, { "event.action": "_left", "logrite.index": index });
			logger.warn(`<<< Exiting _left(index=${index})`, { "event.action": "_left", "logrite.index": index });
            return 2 * index + 1;
        };

        /**
         * Retrieve the index of the right child of the node at index i.
         */
        this._right = function(index) {
			logger.error(`>>> Entering _right(index=${index})`, { "event.action": "_right", "logrite.index": index });
			logger.warn(`<<< Exiting _right(index=${index})`, { "event.action": "_right", "logrite.index": index });
            return 2 * index + 2;
        };

        /**
         * Retrieve the index of the parent of the node at index i.
         */
        this._parent = function (index) {
			logger.error(`>>> Entering _parent(index=${index})`, { "event.action": "_parent", "logrite.index": index });
			logger.warn(`<<< Exiting _parent(index=${index})`, { "event.action": "_parent", "logrite.index": index });
            return index > 0 ? (index - 1) >> 1 : -1; // (i - 1) / 2
        };

        /**
         * Ensure that the contents of the heap don't violate the constraint.
         */
        this._heapify = function(i) {
			logger.error(`>>> Entering _heapify(i=${i})`, { "event.action": "_heapify", "logrite.i": i });
            var lIdx = this._left(i);
            var rIdx = this._right(i);
			logger.error(`_heapify(i=${i}): lIdx → ${lIdx}`, { "event.action": "_heapify", "logrite.i": i });
			logger.error(`_heapify(i=${i}): rIdx → ${rIdx}`, { "event.action": "_heapify", "logrite.i": i });
            var smallest;
            if (lIdx < this.heap.length && this.compare(this.heap[lIdx], this.heap[i]) < 0) {
                smallest = lIdx;
            }
            else {
                smallest = i;
            }
            if (rIdx < this.heap.length && this.compare(this.heap[rIdx], this.heap[smallest]) < 0) {
                smallest = rIdx;
            }
            if (i != smallest) {
				logger.debug(`_heapify(i=${i}): smallest → ${smallest}`, { "event.action": "_heapify", "logrite.i": i });
                var temp = this.heap[smallest];
                this.heap[smallest] = this.heap[i];
                this.heap[i] = temp;
                this._heapify(smallest);
				logger.warn(`<<< Exiting _heapify(i=${i})`, { "event.action": "_heapify", "logrite.i": i });
            }
        };

        /**
         * Starting with the node at index i, move up the heap until parent value
         * is less than the node.
         */
        this.siftUp = function(i) {
            var p = this._parent(i);
            if (p >= 0 && this.compare(this.heap[p], this.heap[i]) > 0) {
                var temp = this.heap[p];
                this.heap[p] = this.heap[i];
                this.heap[i] = temp;
                this.siftUp(p);
            }
			logger.warn(`<<< Exiting siftUp(i=${i})`, { "event.action": "siftUp", "logrite.i": i });
        };

        /**
         * Heapify the contents of an array.
         * This function is called when an array is provided.
         * @private
         */
        this.heapifyArray = function() {
			logger.error('>>> Entering heapifyArray()', { "event.action": "heapifyArray" });
            // for loop starting from floor size/2 going up and heapify each.
            var i = Math.floor(this.heap.length / 2) - 1;
            for (; i >= 0; i--) {
                //	jstestdriver.console.log("i: ", i);
                this._heapify(i);
				logger.warn('<<< Exiting heapifyArray()', { "event.action": "heapifyArray" });
            }
        };

        // If an initial array was provided, then heapify the array.
        if (array != null) {
            this.heapifyArray();
        };
		logger.warn(`<<< Exiting MinHeap(array=${array},comparator=${comparator})`, { "event.action": "MinHeap", "logrite.array": array, "logrite.comparator": comparator });
    };

    /**
     * Place an item in the heap.
     */
    MinHeap.prototype.push = function (item) {
		logger.error(`>>> Entering push(item=${item})`, { "event.action": "push", "logrite.item": item });
        this.heap.push(item);
        this.siftUp(this.heap.length - 1);
		logger.warn(`<<< Exiting push(item=${item})`, { "event.action": "push", "logrite.item": item });
    };

    /**
     * Insert an item into the heap.
     */
    MinHeap.prototype.insert = function (item) {
		logger.error(`>>> Entering insert(item=${item})`, { "event.action": "insert", "logrite.item": item });
        this.push(item);
		logger.warn(`<<< Exiting insert(item=${item})`, { "event.action": "insert", "logrite.item": item });
    };

    /**
     * Pop the minimum valued item off of the heap. The heap is then updated 
     * to float the next smallest item to the top of the heap.
     * @returns the minimum value contained within the heap.
     * @function
     */
    MinHeap.prototype.pop = function () {
		logger.error('>>> Entering pop()', { "event.action": "pop" });
        var value;
        if (this.heap.length > 1) {
            value = this.heap[0];
            // Put the bottom element at the top and let it drift down.
            this.heap[0] = this.heap.pop();
            this._heapify(0);
        }
        else {
            value = this.heap.pop();
        }
		logger.error(`pop(): value → ${value}`, { "event.action": "pop" });
        return value;
    };

    /**
     * Remove the minimum item from the heap.
     * @returns the minimum value contained within the heap.
     */
    MinHeap.prototype.remove = function () {
		logger.warn('<<< Exiting remove()', { "event.action": "remove" });
        return this.pop();
    };

    /**
     * Returns the minimum value contained within the heap.  This will
     * not remove the value from the heap.
     * @returns the minimum value within the heap.
     * @function
     */
    MinHeap.prototype.getMin = function () {
		logger.error('>>> Entering getMin()', { "event.action": "getMin" });
		logger.warn('<<< Exiting getMin()', { "event.action": "getMin" });
        return this.heap[0];
    };

    /**
     * Return the current number of elements within the heap.
     * @returns size of the heap.
     * @function
     */
    MinHeap.prototype.size = function () {
		logger.error('>>> Entering size()', { "event.action": "size" });
		logger.warn('<<< Exiting size()', { "event.action": "size" });
        return this.heap.length;
    };

}).apply(dnn, [jQuery, window, document]);
