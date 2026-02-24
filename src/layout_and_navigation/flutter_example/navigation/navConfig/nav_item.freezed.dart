// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'nav_item.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

/// @nodoc
mixin _$NavItem {
  String get id;
  String get path;
  TranslatableString get label;
  WidgetBuilder get builder;
  TranslatableString? get labelShort;
  NavItem? get parent;
  List<NavItem> get children;
  NavMetadata? get metadata;

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @pragma('vm:prefer-inline')
  $NavItemCopyWith<NavItem> get copyWith =>
      _$NavItemCopyWithImpl<NavItem>(this as NavItem, _$identity);

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is NavItem &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.path, path) || other.path == path) &&
            (identical(other.label, label) || other.label == label) &&
            (identical(other.builder, builder) || other.builder == builder) &&
            (identical(other.labelShort, labelShort) ||
                other.labelShort == labelShort) &&
            (identical(other.parent, parent) || other.parent == parent) &&
            const DeepCollectionEquality().equals(other.children, children) &&
            (identical(other.metadata, metadata) ||
                other.metadata == metadata));
  }

  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    path,
    label,
    builder,
    labelShort,
    parent,
    const DeepCollectionEquality().hash(children),
    metadata,
  );

  @override
  String toString() {
    return 'NavItem(id: $id, path: $path, label: $label, builder: $builder, labelShort: $labelShort, parent: $parent, children: $children, metadata: $metadata)';
  }
}

/// @nodoc
abstract mixin class $NavItemCopyWith<$Res> {
  factory $NavItemCopyWith(NavItem value, $Res Function(NavItem) _then) =
      _$NavItemCopyWithImpl;
  @useResult
  $Res call({
    String id,
    String path,
    TranslatableString label,
    WidgetBuilder builder,
    TranslatableString? labelShort,
    NavItem? parent,
    List<NavItem> children,
    NavMetadata? metadata,
  });

  $TranslatableStringCopyWith<$Res> get label;
  $TranslatableStringCopyWith<$Res>? get labelShort;
  $NavItemCopyWith<$Res>? get parent;
  $NavMetadataCopyWith<$Res>? get metadata;
}

/// @nodoc
class _$NavItemCopyWithImpl<$Res> implements $NavItemCopyWith<$Res> {
  _$NavItemCopyWithImpl(this._self, this._then);

  final NavItem _self;
  final $Res Function(NavItem) _then;

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? path = null,
    Object? label = null,
    Object? builder = null,
    Object? labelShort = freezed,
    Object? parent = freezed,
    Object? children = null,
    Object? metadata = freezed,
  }) {
    return _then(
      _self.copyWith(
        id: null == id
            ? _self.id
            : id // ignore: cast_nullable_to_non_nullable
                  as String,
        path: null == path
            ? _self.path
            : path // ignore: cast_nullable_to_non_nullable
                  as String,
        label: null == label
            ? _self.label
            : label // ignore: cast_nullable_to_non_nullable
                  as TranslatableString,
        builder: null == builder
            ? _self.builder
            : builder // ignore: cast_nullable_to_non_nullable
                  as WidgetBuilder,
        labelShort: freezed == labelShort
            ? _self.labelShort
            : labelShort // ignore: cast_nullable_to_non_nullable
                  as TranslatableString?,
        parent: freezed == parent
            ? _self.parent
            : parent // ignore: cast_nullable_to_non_nullable
                  as NavItem?,
        children: null == children
            ? _self.children
            : children // ignore: cast_nullable_to_non_nullable
                  as List<NavItem>,
        metadata: freezed == metadata
            ? _self.metadata
            : metadata // ignore: cast_nullable_to_non_nullable
                  as NavMetadata?,
      ),
    );
  }

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $TranslatableStringCopyWith<$Res> get label {
    return $TranslatableStringCopyWith<$Res>(_self.label, (value) {
      return _then(_self.copyWith(label: value));
    });
  }

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $TranslatableStringCopyWith<$Res>? get labelShort {
    if (_self.labelShort == null) {
      return null;
    }

    return $TranslatableStringCopyWith<$Res>(_self.labelShort!, (value) {
      return _then(_self.copyWith(labelShort: value));
    });
  }

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $NavItemCopyWith<$Res>? get parent {
    if (_self.parent == null) {
      return null;
    }

    return $NavItemCopyWith<$Res>(_self.parent!, (value) {
      return _then(_self.copyWith(parent: value));
    });
  }

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $NavMetadataCopyWith<$Res>? get metadata {
    if (_self.metadata == null) {
      return null;
    }

    return $NavMetadataCopyWith<$Res>(_self.metadata!, (value) {
      return _then(_self.copyWith(metadata: value));
    });
  }
}

/// Adds pattern-matching-related methods to [NavItem].
extension NavItemPatterns on NavItem {
  /// A variant of `map` that fallback to returning `orElse`.
  ///
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case final Subclass value:
  ///     return ...;
  ///   case _:
  ///     return orElse();
  /// }
  /// ```

  @optionalTypeArgs
  TResult maybeMap<TResult extends Object?>(
    TResult Function(_NavItem value)? $default, {
    required TResult orElse(),
  }) {
    final _that = this;
    switch (_that) {
      case _NavItem() when $default != null:
        return $default(_that);
      case _:
        return orElse();
    }
  }

  /// A `switch`-like method, using callbacks.
  ///
  /// Callbacks receives the raw object, upcasted.
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case final Subclass value:
  ///     return ...;
  ///   case final Subclass2 value:
  ///     return ...;
  /// }
  /// ```

  @optionalTypeArgs
  TResult map<TResult extends Object?>(
    TResult Function(_NavItem value) $default,
  ) {
    final _that = this;
    switch (_that) {
      case _NavItem():
        return $default(_that);
    }
  }

  /// A variant of `map` that fallback to returning `null`.
  ///
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case final Subclass value:
  ///     return ...;
  ///   case _:
  ///     return null;
  /// }
  /// ```

  @optionalTypeArgs
  TResult? mapOrNull<TResult extends Object?>(
    TResult? Function(_NavItem value)? $default,
  ) {
    final _that = this;
    switch (_that) {
      case _NavItem() when $default != null:
        return $default(_that);
      case _:
        return null;
    }
  }

  /// A variant of `when` that fallback to an `orElse` callback.
  ///
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case Subclass(:final field):
  ///     return ...;
  ///   case _:
  ///     return orElse();
  /// }
  /// ```

  @optionalTypeArgs
  TResult maybeWhen<TResult extends Object?>(
    TResult Function(
      String id,
      String path,
      TranslatableString label,
      WidgetBuilder builder,
      TranslatableString? labelShort,
      NavItem? parent,
      List<NavItem> children,
      NavMetadata? metadata,
    )?
    $default, {
    required TResult orElse(),
  }) {
    final _that = this;
    switch (_that) {
      case _NavItem() when $default != null:
        return $default(
          _that.id,
          _that.path,
          _that.label,
          _that.builder,
          _that.labelShort,
          _that.parent,
          _that.children,
          _that.metadata,
        );
      case _:
        return orElse();
    }
  }

  /// A `switch`-like method, using callbacks.
  ///
  /// As opposed to `map`, this offers destructuring.
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case Subclass(:final field):
  ///     return ...;
  ///   case Subclass2(:final field2):
  ///     return ...;
  /// }
  /// ```

  @optionalTypeArgs
  TResult when<TResult extends Object?>(
    TResult Function(
      String id,
      String path,
      TranslatableString label,
      WidgetBuilder builder,
      TranslatableString? labelShort,
      NavItem? parent,
      List<NavItem> children,
      NavMetadata? metadata,
    )
    $default,
  ) {
    final _that = this;
    switch (_that) {
      case _NavItem():
        return $default(
          _that.id,
          _that.path,
          _that.label,
          _that.builder,
          _that.labelShort,
          _that.parent,
          _that.children,
          _that.metadata,
        );
    }
  }

  /// A variant of `when` that fallback to returning `null`
  ///
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case Subclass(:final field):
  ///     return ...;
  ///   case _:
  ///     return null;
  /// }
  /// ```

  @optionalTypeArgs
  TResult? whenOrNull<TResult extends Object?>(
    TResult? Function(
      String id,
      String path,
      TranslatableString label,
      WidgetBuilder builder,
      TranslatableString? labelShort,
      NavItem? parent,
      List<NavItem> children,
      NavMetadata? metadata,
    )?
    $default,
  ) {
    final _that = this;
    switch (_that) {
      case _NavItem() when $default != null:
        return $default(
          _that.id,
          _that.path,
          _that.label,
          _that.builder,
          _that.labelShort,
          _that.parent,
          _that.children,
          _that.metadata,
        );
      case _:
        return null;
    }
  }
}

/// @nodoc

class _NavItem implements NavItem {
  const _NavItem({
    required this.id,
    required this.path,
    required this.label,
    required this.builder,
    this.labelShort,
    this.parent,
    final List<NavItem> children = const [],
    this.metadata,
  }) : _children = children;

  @override
  final String id;
  @override
  final String path;
  @override
  final TranslatableString label;
  @override
  final WidgetBuilder builder;
  @override
  final TranslatableString? labelShort;
  @override
  final NavItem? parent;
  final List<NavItem> _children;
  @override
  @JsonKey()
  List<NavItem> get children {
    if (_children is EqualUnmodifiableListView) return _children;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_children);
  }

  @override
  final NavMetadata? metadata;

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  @pragma('vm:prefer-inline')
  _$NavItemCopyWith<_NavItem> get copyWith =>
      __$NavItemCopyWithImpl<_NavItem>(this, _$identity);

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _NavItem &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.path, path) || other.path == path) &&
            (identical(other.label, label) || other.label == label) &&
            (identical(other.builder, builder) || other.builder == builder) &&
            (identical(other.labelShort, labelShort) ||
                other.labelShort == labelShort) &&
            (identical(other.parent, parent) || other.parent == parent) &&
            const DeepCollectionEquality().equals(other._children, _children) &&
            (identical(other.metadata, metadata) ||
                other.metadata == metadata));
  }

  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    path,
    label,
    builder,
    labelShort,
    parent,
    const DeepCollectionEquality().hash(_children),
    metadata,
  );

  @override
  String toString() {
    return 'NavItem(id: $id, path: $path, label: $label, builder: $builder, labelShort: $labelShort, parent: $parent, children: $children, metadata: $metadata)';
  }
}

/// @nodoc
abstract mixin class _$NavItemCopyWith<$Res> implements $NavItemCopyWith<$Res> {
  factory _$NavItemCopyWith(_NavItem value, $Res Function(_NavItem) _then) =
      __$NavItemCopyWithImpl;
  @override
  @useResult
  $Res call({
    String id,
    String path,
    TranslatableString label,
    WidgetBuilder builder,
    TranslatableString? labelShort,
    NavItem? parent,
    List<NavItem> children,
    NavMetadata? metadata,
  });

  @override
  $TranslatableStringCopyWith<$Res> get label;
  @override
  $TranslatableStringCopyWith<$Res>? get labelShort;
  @override
  $NavItemCopyWith<$Res>? get parent;
  @override
  $NavMetadataCopyWith<$Res>? get metadata;
}

/// @nodoc
class __$NavItemCopyWithImpl<$Res> implements _$NavItemCopyWith<$Res> {
  __$NavItemCopyWithImpl(this._self, this._then);

  final _NavItem _self;
  final $Res Function(_NavItem) _then;

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $Res call({
    Object? id = null,
    Object? path = null,
    Object? label = null,
    Object? builder = null,
    Object? labelShort = freezed,
    Object? parent = freezed,
    Object? children = null,
    Object? metadata = freezed,
  }) {
    return _then(
      _NavItem(
        id: null == id
            ? _self.id
            : id // ignore: cast_nullable_to_non_nullable
                  as String,
        path: null == path
            ? _self.path
            : path // ignore: cast_nullable_to_non_nullable
                  as String,
        label: null == label
            ? _self.label
            : label // ignore: cast_nullable_to_non_nullable
                  as TranslatableString,
        builder: null == builder
            ? _self.builder
            : builder // ignore: cast_nullable_to_non_nullable
                  as WidgetBuilder,
        labelShort: freezed == labelShort
            ? _self.labelShort
            : labelShort // ignore: cast_nullable_to_non_nullable
                  as TranslatableString?,
        parent: freezed == parent
            ? _self.parent
            : parent // ignore: cast_nullable_to_non_nullable
                  as NavItem?,
        children: null == children
            ? _self._children
            : children // ignore: cast_nullable_to_non_nullable
                  as List<NavItem>,
        metadata: freezed == metadata
            ? _self.metadata
            : metadata // ignore: cast_nullable_to_non_nullable
                  as NavMetadata?,
      ),
    );
  }

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $TranslatableStringCopyWith<$Res> get label {
    return $TranslatableStringCopyWith<$Res>(_self.label, (value) {
      return _then(_self.copyWith(label: value));
    });
  }

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $TranslatableStringCopyWith<$Res>? get labelShort {
    if (_self.labelShort == null) {
      return null;
    }

    return $TranslatableStringCopyWith<$Res>(_self.labelShort!, (value) {
      return _then(_self.copyWith(labelShort: value));
    });
  }

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $NavItemCopyWith<$Res>? get parent {
    if (_self.parent == null) {
      return null;
    }

    return $NavItemCopyWith<$Res>(_self.parent!, (value) {
      return _then(_self.copyWith(parent: value));
    });
  }

  /// Create a copy of NavItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $NavMetadataCopyWith<$Res>? get metadata {
    if (_self.metadata == null) {
      return null;
    }

    return $NavMetadataCopyWith<$Res>(_self.metadata!, (value) {
      return _then(_self.copyWith(metadata: value));
    });
  }
}

/// @nodoc
mixin _$NavMetadata {
  /// Show this item in navigation menus (header, footer, mobile menu)
  bool get showInNav;

  /// Show this item in breadcrumb trail
  /// Set to false for utility pages (login, 404, etc.)
  bool get showInBreadcrumb;

  /// Sort order in navigation menus (ascending)
  /// Lower numbers appear first (e.g., Home = 1, Explore = 2)
  int get order;

  /// Require authentication to access this route
  /// If true and user not authenticated → redirect to login
  bool get requiresAuth;

  /// Create a copy of NavMetadata
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @pragma('vm:prefer-inline')
  $NavMetadataCopyWith<NavMetadata> get copyWith =>
      _$NavMetadataCopyWithImpl<NavMetadata>(this as NavMetadata, _$identity);

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is NavMetadata &&
            (identical(other.showInNav, showInNav) ||
                other.showInNav == showInNav) &&
            (identical(other.showInBreadcrumb, showInBreadcrumb) ||
                other.showInBreadcrumb == showInBreadcrumb) &&
            (identical(other.order, order) || other.order == order) &&
            (identical(other.requiresAuth, requiresAuth) ||
                other.requiresAuth == requiresAuth));
  }

  @override
  int get hashCode => Object.hash(
    runtimeType,
    showInNav,
    showInBreadcrumb,
    order,
    requiresAuth,
  );

  @override
  String toString() {
    return 'NavMetadata(showInNav: $showInNav, showInBreadcrumb: $showInBreadcrumb, order: $order, requiresAuth: $requiresAuth)';
  }
}

/// @nodoc
abstract mixin class $NavMetadataCopyWith<$Res> {
  factory $NavMetadataCopyWith(
    NavMetadata value,
    $Res Function(NavMetadata) _then,
  ) = _$NavMetadataCopyWithImpl;
  @useResult
  $Res call({
    bool showInNav,
    bool showInBreadcrumb,
    int order,
    bool requiresAuth,
  });
}

/// @nodoc
class _$NavMetadataCopyWithImpl<$Res> implements $NavMetadataCopyWith<$Res> {
  _$NavMetadataCopyWithImpl(this._self, this._then);

  final NavMetadata _self;
  final $Res Function(NavMetadata) _then;

  /// Create a copy of NavMetadata
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? showInNav = null,
    Object? showInBreadcrumb = null,
    Object? order = null,
    Object? requiresAuth = null,
  }) {
    return _then(
      _self.copyWith(
        showInNav: null == showInNav
            ? _self.showInNav
            : showInNav // ignore: cast_nullable_to_non_nullable
                  as bool,
        showInBreadcrumb: null == showInBreadcrumb
            ? _self.showInBreadcrumb
            : showInBreadcrumb // ignore: cast_nullable_to_non_nullable
                  as bool,
        order: null == order
            ? _self.order
            : order // ignore: cast_nullable_to_non_nullable
                  as int,
        requiresAuth: null == requiresAuth
            ? _self.requiresAuth
            : requiresAuth // ignore: cast_nullable_to_non_nullable
                  as bool,
      ),
    );
  }
}

/// Adds pattern-matching-related methods to [NavMetadata].
extension NavMetadataPatterns on NavMetadata {
  /// A variant of `map` that fallback to returning `orElse`.
  ///
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case final Subclass value:
  ///     return ...;
  ///   case _:
  ///     return orElse();
  /// }
  /// ```

  @optionalTypeArgs
  TResult maybeMap<TResult extends Object?>(
    TResult Function(_NavMetadata value)? $default, {
    required TResult orElse(),
  }) {
    final _that = this;
    switch (_that) {
      case _NavMetadata() when $default != null:
        return $default(_that);
      case _:
        return orElse();
    }
  }

  /// A `switch`-like method, using callbacks.
  ///
  /// Callbacks receives the raw object, upcasted.
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case final Subclass value:
  ///     return ...;
  ///   case final Subclass2 value:
  ///     return ...;
  /// }
  /// ```

  @optionalTypeArgs
  TResult map<TResult extends Object?>(
    TResult Function(_NavMetadata value) $default,
  ) {
    final _that = this;
    switch (_that) {
      case _NavMetadata():
        return $default(_that);
    }
  }

  /// A variant of `map` that fallback to returning `null`.
  ///
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case final Subclass value:
  ///     return ...;
  ///   case _:
  ///     return null;
  /// }
  /// ```

  @optionalTypeArgs
  TResult? mapOrNull<TResult extends Object?>(
    TResult? Function(_NavMetadata value)? $default,
  ) {
    final _that = this;
    switch (_that) {
      case _NavMetadata() when $default != null:
        return $default(_that);
      case _:
        return null;
    }
  }

  /// A variant of `when` that fallback to an `orElse` callback.
  ///
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case Subclass(:final field):
  ///     return ...;
  ///   case _:
  ///     return orElse();
  /// }
  /// ```

  @optionalTypeArgs
  TResult maybeWhen<TResult extends Object?>(
    TResult Function(
      bool showInNav,
      bool showInBreadcrumb,
      int order,
      bool requiresAuth,
    )?
    $default, {
    required TResult orElse(),
  }) {
    final _that = this;
    switch (_that) {
      case _NavMetadata() when $default != null:
        return $default(
          _that.showInNav,
          _that.showInBreadcrumb,
          _that.order,
          _that.requiresAuth,
        );
      case _:
        return orElse();
    }
  }

  /// A `switch`-like method, using callbacks.
  ///
  /// As opposed to `map`, this offers destructuring.
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case Subclass(:final field):
  ///     return ...;
  ///   case Subclass2(:final field2):
  ///     return ...;
  /// }
  /// ```

  @optionalTypeArgs
  TResult when<TResult extends Object?>(
    TResult Function(
      bool showInNav,
      bool showInBreadcrumb,
      int order,
      bool requiresAuth,
    )
    $default,
  ) {
    final _that = this;
    switch (_that) {
      case _NavMetadata():
        return $default(
          _that.showInNav,
          _that.showInBreadcrumb,
          _that.order,
          _that.requiresAuth,
        );
    }
  }

  /// A variant of `when` that fallback to returning `null`
  ///
  /// It is equivalent to doing:
  /// ```dart
  /// switch (sealedClass) {
  ///   case Subclass(:final field):
  ///     return ...;
  ///   case _:
  ///     return null;
  /// }
  /// ```

  @optionalTypeArgs
  TResult? whenOrNull<TResult extends Object?>(
    TResult? Function(
      bool showInNav,
      bool showInBreadcrumb,
      int order,
      bool requiresAuth,
    )?
    $default,
  ) {
    final _that = this;
    switch (_that) {
      case _NavMetadata() when $default != null:
        return $default(
          _that.showInNav,
          _that.showInBreadcrumb,
          _that.order,
          _that.requiresAuth,
        );
      case _:
        return null;
    }
  }
}

/// @nodoc

class _NavMetadata implements NavMetadata {
  const _NavMetadata({
    this.showInNav = true,
    this.showInBreadcrumb = true,
    this.order = 0,
    this.requiresAuth = false,
  });

  /// Show this item in navigation menus (header, footer, mobile menu)
  @override
  @JsonKey()
  final bool showInNav;

  /// Show this item in breadcrumb trail
  /// Set to false for utility pages (login, 404, etc.)
  @override
  @JsonKey()
  final bool showInBreadcrumb;

  /// Sort order in navigation menus (ascending)
  /// Lower numbers appear first (e.g., Home = 1, Explore = 2)
  @override
  @JsonKey()
  final int order;

  /// Require authentication to access this route
  /// If true and user not authenticated → redirect to login
  @override
  @JsonKey()
  final bool requiresAuth;

  /// Create a copy of NavMetadata
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  @pragma('vm:prefer-inline')
  _$NavMetadataCopyWith<_NavMetadata> get copyWith =>
      __$NavMetadataCopyWithImpl<_NavMetadata>(this, _$identity);

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _NavMetadata &&
            (identical(other.showInNav, showInNav) ||
                other.showInNav == showInNav) &&
            (identical(other.showInBreadcrumb, showInBreadcrumb) ||
                other.showInBreadcrumb == showInBreadcrumb) &&
            (identical(other.order, order) || other.order == order) &&
            (identical(other.requiresAuth, requiresAuth) ||
                other.requiresAuth == requiresAuth));
  }

  @override
  int get hashCode => Object.hash(
    runtimeType,
    showInNav,
    showInBreadcrumb,
    order,
    requiresAuth,
  );

  @override
  String toString() {
    return 'NavMetadata(showInNav: $showInNav, showInBreadcrumb: $showInBreadcrumb, order: $order, requiresAuth: $requiresAuth)';
  }
}

/// @nodoc
abstract mixin class _$NavMetadataCopyWith<$Res>
    implements $NavMetadataCopyWith<$Res> {
  factory _$NavMetadataCopyWith(
    _NavMetadata value,
    $Res Function(_NavMetadata) _then,
  ) = __$NavMetadataCopyWithImpl;
  @override
  @useResult
  $Res call({
    bool showInNav,
    bool showInBreadcrumb,
    int order,
    bool requiresAuth,
  });
}

/// @nodoc
class __$NavMetadataCopyWithImpl<$Res> implements _$NavMetadataCopyWith<$Res> {
  __$NavMetadataCopyWithImpl(this._self, this._then);

  final _NavMetadata _self;
  final $Res Function(_NavMetadata) _then;

  /// Create a copy of NavMetadata
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $Res call({
    Object? showInNav = null,
    Object? showInBreadcrumb = null,
    Object? order = null,
    Object? requiresAuth = null,
  }) {
    return _then(
      _NavMetadata(
        showInNav: null == showInNav
            ? _self.showInNav
            : showInNav // ignore: cast_nullable_to_non_nullable
                  as bool,
        showInBreadcrumb: null == showInBreadcrumb
            ? _self.showInBreadcrumb
            : showInBreadcrumb // ignore: cast_nullable_to_non_nullable
                  as bool,
        order: null == order
            ? _self.order
            : order // ignore: cast_nullable_to_non_nullable
                  as int,
        requiresAuth: null == requiresAuth
            ? _self.requiresAuth
            : requiresAuth // ignore: cast_nullable_to_non_nullable
                  as bool,
      ),
    );
  }
}
