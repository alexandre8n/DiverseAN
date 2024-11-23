def flatten(lst):
    flat_list = []
    for item in lst:
        if isinstance(item, list):
            flat_list.extend(flatten(item))  # Recursively flatten the nested list
        else:
            flat_list.append(item)
    return flat_list

def max_value(self, value):
    num_set = set(value)
    max_value = None
    for num in value:
        if -num in num_set:
            if max_value is None or abs(num) > abs(max_value):
                max_value = num
    return max_value

value = [ 4, 1, 2, -7, 5, -4, -1]
some_resulr = max_value(value)

# Example usage
lst = [1, 2, 3, [[4], [5,6],7]]
flattened_list = flatten(lst)
print(flattened_list)

